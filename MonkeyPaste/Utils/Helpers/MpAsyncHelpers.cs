﻿using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace MonkeyPaste {
    public static class MpAsyncHelpers {
        // from https://stackoverflow.com/a/5097066/105028

        /// <summary>
        /// Execute's an async Task<T> method which has a void return paramValue synchronously
        /// </summary>
        /// <param name="task">Task<T> method to execute</param>
        public static void RunSync(Func<Task> task) {
            var oldContext = SynchronizationContext.Current;
            var synch = new ExclusiveSynchronizationContext();
            SynchronizationContext.SetSynchronizationContext(synch);
#pragma warning disable VSTHRD101 // Avoid unsupported async delegates
            synch.Post(async _ => {
                try {
                    await task();
                }
                catch (Exception e) {
                    synch.InnerException = e;
                    throw;
                }
                finally {
                    synch.EndMessageLoop();
                }
            }, null);
#pragma warning restore VSTHRD101 // Avoid unsupported async delegates
            synch.BeginMessageLoop();

            SynchronizationContext.SetSynchronizationContext(oldContext);
        }


        /// <summary>
        /// Execute's an async Task<T> method which has a T return type synchronously
        /// </summary>
        /// <typeparam name="T">Return Type</typeparam>
        /// <param name="func">Task<T> method to execute</param>
        /// <returns></returns>
        public static T RunSync<T>(Func<T> func) {
            var oldContext = SynchronizationContext.Current;
            var synch = new ExclusiveSynchronizationContext();
            SynchronizationContext.SetSynchronizationContext(synch);
            T ret = default(T);
#pragma warning disable VSTHRD101 // Avoid unsupported async delegates
            synch.Post(_ => {
                try {
                    ret = func();
                }
                catch (Exception e) {
                    synch.InnerException = e;
                    throw;
                }
                finally {
                    synch.EndMessageLoop();
                }
            }, null);
#pragma warning restore VSTHRD101 // Avoid unsupported async delegates
            synch.BeginMessageLoop();
            SynchronizationContext.SetSynchronizationContext(oldContext);
            return ret;
        }

        public static T RunSync<T>(Func<Task<T>> task) {
            var oldContext = SynchronizationContext.Current;
            var synch = new ExclusiveSynchronizationContext();
            SynchronizationContext.SetSynchronizationContext(synch);
            T ret = default(T);
#pragma warning disable VSTHRD101 // Avoid unsupported async delegates
            synch.Post(async _ => {
                try {
                    ret = await task();
                }
                catch (Exception e) {
                    synch.InnerException = e;
                    throw;
                }
                finally {
                    synch.EndMessageLoop();
                }
            }, null);
#pragma warning restore VSTHRD101 // Avoid unsupported async delegates
            synch.BeginMessageLoop();
            SynchronizationContext.SetSynchronizationContext(oldContext);
            return ret;
        }

        private class ExclusiveSynchronizationContext : SynchronizationContext {
            private bool done;
            public Exception InnerException { get; set; }
            readonly AutoResetEvent workItemsWaiting = new AutoResetEvent(false);
            readonly Queue<Tuple<SendOrPostCallback, object>> items =
                new Queue<Tuple<SendOrPostCallback, object>>();

            public override void Send(SendOrPostCallback d, object state) {
                throw new NotSupportedException("We cannot send to our same thread");
            }

            public override void Post(SendOrPostCallback d, object state) {
                lock (items) {
                    items.Enqueue(Tuple.Create(d, state));
                }
                workItemsWaiting.Set();
            }

            public void EndMessageLoop() {
                Post(_ => done = true, null);
            }

            public void BeginMessageLoop() {
                while (!done) {
                    Tuple<SendOrPostCallback, object> task = null;
                    lock (items) {
                        if (items.Count > 0) {
                            task = items.Dequeue();
                        }
                    }
                    if (task != null) {
                        task.Item1(task.Item2);
                        if (InnerException != null) // the method threw an exeption
                        {
                            throw new AggregateException("MpAsyncHelpers.Run method threw an exception.", InnerException);
                        }
                    } else {
                        workItemsWaiting.WaitOne();
                    }
                }
            }

            public override SynchronizationContext CreateCopy() {
                return this;
            }
        }
    }
}
