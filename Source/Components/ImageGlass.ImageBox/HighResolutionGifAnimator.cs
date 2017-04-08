using System;
using System.Collections.Concurrent;
using System.Drawing;
using System.Drawing.Imaging;
using System.Threading;

namespace ImageGlass {

    /// <summary>
    /// <p> Implements GifAnimator with the potential to offer a timer resolution of 10ms,
    /// the fastest a GIF can animate. </p>  
    /// <p> Each animated image is given its own thread
    /// which is torn down with a corresponding call to StopAnimate or when the spawning
    /// process dies. The default resolution is 20ms, as windows timers are by default limited
    /// to a resolution of 15ms.  Call setTickInMilliseconds to ask for a different rate, which
    /// sets the fastest tick allowed for all HighResolutionAnimators. </p>
    /// </summary>
    public class HighResolutionGifAnimator : GifAnimator {
        #region STATIC 
        private static int ourMinTickTimeInMilliseconds;
        private static readonly ConcurrentDictionary<Image, GifImageData> ourImageState;

        /// <summary>
        /// Sets the tick for the animation thread. The thread may use a lower tick to ensure
        /// the passed value is divisible by 10 (the gif format operates in units of 10 ms).
        /// </summary>
        /// <param name="value"> Ideally should be a multiple of 10. </param>
        /// <returns>The actual tick value that will be used</returns>
        public static int setTickTimeInMilliseconds(int value) {
            // 10 is the minimum value, as a GIF's lowest tick rate is 10ms 
            //
            int newTickValue = Math.Max(10, (value / 10) * 10);
            ourMinTickTimeInMilliseconds = newTickValue;
            return newTickValue;
        }

        public static int getTickTimeInMilliseconds() {
            return ourMinTickTimeInMilliseconds;
        }

        /// <summary>
        /// Given a delay amount, return either the minimum tick or delay, whichever is greater.
        /// </summary>
        /// <returns> the time to sleep during a tick in milliseconds </returns>
        private static int getSleepAmountInMilliseconds(int delayInMilliseconds) {
            return Math.Max(ourMinTickTimeInMilliseconds, delayInMilliseconds);
        }

        static HighResolutionGifAnimator() {
            ourMinTickTimeInMilliseconds = 20;
            ourImageState = new ConcurrentDictionary<Image, GifImageData>();
        }
        #endregion

        public void Animate(Image image, EventHandler onFrameChangedHandler) {
            if (image == null)
                return;

            if (ourImageState.ContainsKey(image))
                return;

            // AddOrUpdate has a race condition that could allow us to erroneously
            // create multiple animation threads per image.  To combat that
            // we manually try to add entries ourself, and if it fails we 
            // kill the thread.
            //
            GifImageData toAdd = addFactory(image, onFrameChangedHandler);
            if (!ourImageState.TryAdd(image, toAdd))
                Interlocked.Exchange(ref toAdd.myIsThreadDead, 1);
        }

        private GifImageData addFactory(Image image, EventHandler eventHandler) {
            GifImageData data;
            lock (image) {
                data = new GifImageData(image, eventHandler);
            }

            Thread heartbeat = new Thread(() => {
                int sleepTime = getSleepAmountInMilliseconds(data.getCurrentDelayInMilliseconds());
                Thread.Sleep(sleepTime);
                while (data.threadIsNotDead()) {
                    data.handleUpdateTick();
                    sleepTime = getSleepAmountInMilliseconds(data.getCurrentDelayInMilliseconds());
                    Thread.Sleep(sleepTime);
                }
            });
            heartbeat.IsBackground = true;
            heartbeat.Name = "heartbeat - HighResolutionAnimator";
            heartbeat.Start();
            return data;
        }

        public void UpdateFrames(Image image) {
            if (image == null)
                return;

            GifImageData outData;
            if (!ourImageState.TryGetValue(image, out outData))
                return;

            if (!outData.myIsDirty)
                return;

            lock (image) {
                outData.updateFrame();
            }
        }

        public void StopAnimate(Image image, EventHandler eventHandler) {
            if (image == null)
                return;

            GifImageData outData;
            if (ourImageState.TryRemove(image, out outData))
                Interlocked.Exchange(ref outData.myIsThreadDead, 1);
        }

        // See if we have more than one frame in the time dimension.
        //
        public bool CanAnimate(Image image) {
            if (image == null)
                return false;

            if (ourImageState.ContainsKey(image))
                return true;

            lock (image) {
                if (imageHasTimeFrames(image))
                    return image.GetFrameCount(FrameDimension.Time) > 1;
            }

            return false;
        }

        // image lock should be held
        //
        private bool imageHasTimeFrames(Image image) {
            foreach (Guid guid in image.FrameDimensionsList) {
                FrameDimension dimension = new FrameDimension(guid);
                if (dimension.Equals(FrameDimension.Time))
                    return image.GetFrameCount(FrameDimension.Time) > 1;
            }

            return false;
        }

        private class GifImageData {
            private static readonly int FrameDelayTag = 0x5100;

            // image is used for identification in map
            //
            public int myIsThreadDead;

            private readonly Image myImage;
            private readonly EventHandler myOnFrameChangedHandler;
            private readonly int myNumFrames;
            private readonly int[] myFrameDelaysInCentiseconds;
            public bool myIsDirty;
            private int myCurrentFrame;

            // image should be locked by caller
            //
            public GifImageData(Image image, EventHandler onFrameChangedHandler) {
                myIsThreadDead = 0;
                myImage = image;
                // We should only be called if we already know we can be animated. Therefore this
                // call is valid.
                //
                myNumFrames = image.GetFrameCount(FrameDimension.Time);
                myFrameDelaysInCentiseconds = new int[myNumFrames];
                populateFrameDelays(image);
                myCurrentFrame = 0;
                myIsDirty = false;
                myOnFrameChangedHandler = onFrameChangedHandler;
            }

            public bool threadIsNotDead() {
                return myIsThreadDead == 0;
            }

            public void handleUpdateTick() {
                myCurrentFrame = (myCurrentFrame + 1) % myNumFrames;
                myIsDirty = true;
                myOnFrameChangedHandler(myImage, EventArgs.Empty);
            }

            public int getCurrentDelayInMilliseconds() {
                return myFrameDelaysInCentiseconds[myCurrentFrame] * 10;
            }

            public void updateFrame() {
                myImage.SelectActiveFrame(FrameDimension.Time, myCurrentFrame);
            }

            private void populateFrameDelays(Image image) {
                byte[] frameDelays = image.GetPropertyItem(FrameDelayTag).Value;
                for (int i = 0; i < myNumFrames; i++) {
                    myFrameDelaysInCentiseconds[i] = BitConverter.ToInt32(frameDelays, i * 4);
                    // Sometimes gifs have a zero frame delay, erroneously?
                    // These gifs seem to play differently depending on the program.
                    // I'll give them a default delay, as most gifs with 0 delay seem
                    // wayyyy to fast compared to other programs.
                    //
                    // 0.1 seconds appears to be chromes default setting... I'll use that
                    // 
                    if (myFrameDelaysInCentiseconds[i] < 1)
                        myFrameDelaysInCentiseconds[i] = 10;
                }
            }
        }
    }
}