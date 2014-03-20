/*
ImageGlass Project - Image viewer for Windows
Copyright (C) 2014 DUONG DIEU PHAP
Project homepage: http://imageglass.org

This program is free software: you can redistribute it and/or modify
it under the terms of the GNU General Public License as published by
the Free Software Foundation, either version 3 of the License, or
(at your option) any later version.

This program is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
GNU General Public License for more details.

You should have received a copy of the GNU General Public License
along with this program.  If not, see <http://www.gnu.org/licenses/>.
*/
using System;
using System.Threading;


namespace ImageGlass.ThumbBar
{
    
    /// <summary>
    /// Class that partitions a list of tasks to be handled by as many threads as the processor cores available.
    /// </summary>
    class TasksDispatcher
    {

        static TasksDispatcher()
        {
            ProcessorCount = Environment.ProcessorCount;

            IndividualTaskEvents = new AutoResetEvent[ProcessorCount];
            for (int i = 0; i < ProcessorCount; i++)
                IndividualTaskEvents[i] = new AutoResetEvent(false);
        }


        public TasksDispatcher(WaitCallback globalTask, WaitCallback individualTask, int tasksCount)
        {
            this.globalTask = globalTask;
            this.individualTask = individualTask;
            this.tasksCount = tasksCount;
        }


        public void Start()
        {
            dispatcherIsActive = true;
            
            ThreadPool.QueueUserWorkItem(WorkerThreadLoopMethod);
        }


        public void Stop()
        {
            dispatcherIsActive = false;
        }


        #region Private

        private static readonly int ProcessorCount;
        private static AutoResetEvent[] IndividualTaskEvents;

        private WaitCallback globalTask;
        private WaitCallback individualTask;
        private int tasksCount;

        private volatile bool dispatcherIsActive;


        private void WorkerThreadLoopMethod(object state)
        {
            try
            {
                for (int i = 0; (i < tasksCount) && (dispatcherIsActive); i += ProcessorCount)
                {
                    int j;

                    for (j = 0; (j < ProcessorCount) && (i + j < tasksCount) && (dispatcherIsActive); j++)
                        globalTask(i + j);

                    for (j = 0; (j < ProcessorCount) && (i + j < tasksCount) && (dispatcherIsActive); j++)
                        ThreadPool.QueueUserWorkItem(IndividualTaskWrapper, new TaskParameter() { mainIndex = i + j, threadIndex = j });

                    for (j = 0; (j < ProcessorCount) && (i + j < tasksCount) && (dispatcherIsActive); j++)
                        IndividualTaskEvents[j].Set();
                }
            }
            catch
            {
            }
        }

        private void IndividualTaskWrapper(object state)
        {
            try
            {
                TaskParameter taskParam = ((TaskParameter)state);

                IndividualTaskEvents[taskParam.threadIndex].WaitOne();
                individualTask(taskParam.mainIndex);
            }
            catch
            {
            }
        }

        #endregion

    }

}
