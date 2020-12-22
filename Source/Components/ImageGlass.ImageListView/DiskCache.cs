using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;

namespace ImageGlass.ImageListView {
    /// <summary>
    /// Represents a collection of items on disk that can be read 
    /// and written by multiple threads.
    /// </summary>
    internal class DiskCache: IDisposable {
        #region Enums
        /// <summary>
        /// Represents the synchronization behaviour.
        /// </summary>
        [Flags]
        public enum SyncBehavior {
            /// <summary>
            /// A minimal number of locking is performed.
            /// Both reads and writes may result in cache misses.
            /// </summary>
            SyncNone = 0,
            /// <summary>
            /// Cache reads are synchronized.
            /// </summary>
            SyncReads = 1,
            /// <summary>
            /// Cache writes are synchronized.
            /// </summary>
            SyncWrites = 2,
            /// <summary>
            /// Both cache reads and writes are synchronized.
            /// </summary>
            SnycAll = SyncReads | SyncWrites
        }
        #endregion

        #region Member Variables
        private string mFileName;
        private long mSize;
        private SyncBehavior mSyncBehavior;
        private int mKeySize;

        private FileStream stream;
        private Dictionary<string, CacheItem> index;

        private readonly object lockObject;
        private long writeOffset;
        #endregion

        #region Properties
        /// <summary>
        /// Gets or sets the cache file name.
        /// </summary>
        public string FileName {
            get { return mFileName; }
            set {
                mFileName = value;
                if (stream != null)
                    stream.Close();

                if (!string.IsNullOrEmpty(mFileName)) {
                    stream = new FileStream(mFileName, FileMode.OpenOrCreate, FileAccess.ReadWrite);
                    BuildIndex();
                }
            }
        }
        /// <summary>
        /// Gets or sets the maximum size of the cache file in bytes.
        /// </summary>
        public long Size { get { return mSize; } set { mSize = value; } }
        #endregion

        #region CacheItem class
        /// <summary>
        /// Represents an item in the cache.
        /// </summary>
        private struct CacheItem {
            /// <summary>
            /// Gets the item identifier.
            /// </summary>
            public string ID { get; private set; }
            /// <summary>
            /// Gets the offset to the item in the cache file.
            /// </summary>
            public long Offset { get; private set; }
            /// <summary>
            /// Gets the size of item data in bytes.
            /// </summary>
            public long Length { get; private set; }

            /// <summary>
            /// Initializes a new instance of the <see cref="CacheItem"/> class.
            /// </summary>
            /// <param name="id">Item identifier.</param>
            /// <param name="offset">Offset to the item in the cache file.</param>
            /// <param name="length">Size of item data in bytes.</param>
            public CacheItem(string id, long offset, long length)
                : this() {
                ID = id;
                Offset = offset;
                Length = length;
            }

            /// <summary>
            /// Initializes a new instance of the <see cref="CacheItem"/> class.
            /// </summary>
            /// <param name="id">Item identifier.</param>
            public CacheItem(string id)
                : this(id, -1, -1) {
                ;
            }
        }
        #endregion

        #region Constructor
        /// <summary>
        /// Initializes a new instance of the <see cref="DiskCache"/> class.
        /// </summary>
        /// <param name="filename">The path to the cache file.</param>
        /// <param name="size">Maximum cache size in bytes. When this size is exceeded,
        /// old items will be overwritten.</param>
        /// <param name="syncBehavior">The synchronization behaviour.</param>
        /// <param name="keySize">Byte length of keys.</param>
        public DiskCache(string filename, long size, SyncBehavior syncBehavior, int keySize) {
            lockObject = new object();
            writeOffset = 0;

            mKeySize = keySize;
            mSyncBehavior = syncBehavior;

            index = new Dictionary<string, CacheItem>();
            mFileName = filename;
            mSize = size;

            if (!string.IsNullOrEmpty(mFileName)) {
                stream = new FileStream(filename, FileMode.OpenOrCreate, FileAccess.ReadWrite);
                BuildIndex();
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DiskCache"/> class.
        /// </summary>
        /// <param name="filename">The path to the cache file.</param>
        /// <param name="size">Maximum cache size in bytes. When this size is exceeded,
        /// old items will be overwritten.</param>
        public DiskCache(string filename, long size)
            : this(filename, size, SyncBehavior.SnycAll, 32) {
            ;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DiskCache"/> class with
        /// a maximum file size of 100 MiB.
        /// </summary>
        /// <param name="filename">The path to the cache file.</param>
        public DiskCache(string filename)
            : this(filename, 100 * 1024 * 1024, SyncBehavior.SnycAll, 32) {
            ;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DiskCache"/> class.
        /// </summary>
        public DiskCache()
            : this(string.Empty, 100 * 1024 * 1024, SyncBehavior.SnycAll, 32) {
            ;
        }
        #endregion

        #region Instance Methods
        /// <summary>
        /// Performs application-defined tasks associated with 
        /// freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose() {
            if (stream != null)
                stream.Close();
        }

        /// <summary>
        /// Rebuilds the index of items in the cache.
        /// </summary>
        private void BuildIndex() {
            if (stream == null)
                throw new InvalidOperationException();

            Monitor.Enter(lockObject);
            try {
                writeOffset = 0;
                stream.Seek(0, SeekOrigin.Begin);
                while (stream.Position < stream.Length) {
                    int read;
                    byte[] buffer;

                    buffer = new byte[mKeySize];
                    read = stream.Read(buffer, 0, mKeySize);
                    if (read != mKeySize)
                        break;
                    string id = Encoding.ASCII.GetString(buffer);

                    buffer = new byte[8];
                    read = stream.Read(buffer, 0, 8);
                    if (read != 8)
                        break;
                    long length = BitConverter.ToInt64(buffer, 0);

                    CacheItem item = new CacheItem(id, writeOffset, length);
                    if (index.ContainsKey(id))
                        index[id] = item;
                    else
                        index.Add(id, item);

                    stream.Seek(length, SeekOrigin.Current);
                    writeOffset += 24 + length;
                }
            }
            finally {
                Monitor.Exit(lockObject);
            }
        }

        /// <summary>
        /// Reads an item from the cache.
        /// </summary>
        /// <param name="id">Item identifier.</param>
        /// <param name="data">When this function returns, <paramref name="data"/> 
        /// will hold item data.</param>
        /// <returns>True if the item was read; otherwise false.</returns>
        public bool Read(string id, Stream data) {
            if (stream == null)
                return false;
            id = MakeKey(id);

            if ((mSyncBehavior & SyncBehavior.SyncReads) == SyncBehavior.SyncNone) {
                if (!Monitor.TryEnter(lockObject))
                    return false;
            }
            else {
                Monitor.Enter(lockObject);
            }

            try {
                CacheItem item;
                if (!index.TryGetValue(id, out item))
                    return false;

                stream.Seek(item.Offset, SeekOrigin.Begin);

                int read;
                byte[] buffer;

                buffer = new byte[mKeySize];
                read = stream.Read(buffer, 0, mKeySize);
                if (read != mKeySize) {
                    index.Remove(id);
                    return false;
                }
                string checkid = Encoding.ASCII.GetString(buffer);
                if (checkid != item.ID) {
                    index.Remove(id);
                    return false;
                }

                buffer = new byte[8];
                read = stream.Read(buffer, 0, 8);
                if (read != 8) {
                    index.Remove(id);
                    return false;
                }
                long length = BitConverter.ToInt64(buffer, 0);
                if (length != item.Length) {
                    index.Remove(id);
                    return false;
                }

                if (stream.Position + length > stream.Length) {
                    index.Remove(id);
                    return false;
                }

                data.Seek(0, SeekOrigin.Begin);
                data.SetLength(length);
                long totalRead = 0;
                buffer = new byte[4096];
                while (totalRead < length) {
                    read = stream.Read(buffer, 0, 4096);
                    data.Write(buffer, 0, read);
                    totalRead += read;
                }
            }
            finally {
                Monitor.Exit(lockObject);
            }

            return true;
        }

        /// <summary>
        /// Reads an item from the cache.
        /// </summary>
        /// <param name="id">Item identifier.</param>
        /// <param name="data">When this function returns, <paramref name="data"/> 
        /// will hold item data.</param>
        /// <returns>True if the item was read; otherwise false.</returns>
        public bool Read(string id, byte[] data) {
            if (stream == null)
                return false;

            using (MemoryStream dataStream = new MemoryStream(data)) {
                return Read(id, dataStream);
            }
        }

        /// <summary>
        /// Writes an item to the cache.
        /// </summary>
        /// <param name="id">Item identifier. If an item with this identifier already 
        /// exists, it will be overwritten.</param>
        /// <param name="data">Item data.</param>
        /// <returns>True if the item was written; otherwise false.</returns>
        public bool Write(string id, Stream data) {
            if (stream == null)
                return false;
            id = MakeKey(id);

            if ((mSyncBehavior & SyncBehavior.SyncWrites) == SyncBehavior.SyncNone) {
                if (!Monitor.TryEnter(lockObject))
                    return false;
            }
            else {
                Monitor.Enter(lockObject);
            }

            try {
                stream.Seek(writeOffset, SeekOrigin.Begin);
                data.Seek(0, SeekOrigin.Begin);

                byte[] buffer;
                buffer = Encoding.ASCII.GetBytes(id);
                stream.Write(buffer, 0, buffer.Length);
                stream.Write(BitConverter.GetBytes(data.Length), 0, 8);

                int totalRead = 0;
                buffer = new byte[4096];

                while (data.Position < data.Length) {
                    int read = data.Read(buffer, 0, 4096);
                    stream.Write(buffer, 0, read);
                    totalRead += read;
                }

                CacheItem item = new CacheItem(id, writeOffset, totalRead);
                if (index.ContainsKey(id))
                    index[id] = item;
                else
                    index.Add(id, item);

                writeOffset += 24 + totalRead;
                if (writeOffset > mSize)
                    writeOffset = 0;
            }
            finally {
                Monitor.Exit(lockObject);
            }

            return true;
        }

        /// <summary>
        /// Writes an item to the cache.
        /// </summary>
        /// <param name="id">Item identifier. If an item with this identifier already 
        /// exists, it will be overwritten.</param>
        /// <param name="data">Item data.</param>
        /// <returns>True if the item was written; otherwise false.</returns>
        public bool Write(string id, byte[] data) {
            if (stream == null)
                return false;

            using (MemoryStream dataStream = new MemoryStream(data)) {
                return Write(id, dataStream);
            }
        }
        /// <summary>
        /// Converts the given string to an item key.
        /// </summary>
        /// <param name="key">Input string.</param>
        /// <returns>Item key.</returns>
        private string MakeKey(string key) {
            if (key.Length > mKeySize)
                key = key.Substring(0, mKeySize);
            if (key.Length < mKeySize)
                key = key + new string(' ', mKeySize - key.Length);
            return key;
        }
        #endregion
    }
}
