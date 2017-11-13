// Copyright 2017 Google Inc. All rights reserved.
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//     http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

using System;
using System.Collections.Generic;
using System.Threading;

namespace USD.NET {
  internal class BackgroundExecutor {

    public bool Enabled { get; set; }
    public bool Paused { get; set; }

    public System.Text.StringBuilder log = new System.Text.StringBuilder();

    public BackgroundExecutor() {
      Enabled = true;
      Paused = true;
      m_writer = new Thread(new ThreadStart(RunWriterLoop));
      m_writer.IsBackground = true;
      m_writer.Start();

      m_reader = new Thread(new ThreadStart(RunReaderLoop));
      m_reader.IsBackground = true;
      m_reader.Start();
    }

    public void Stop() {
      m_writer.Abort();
      m_reader.Abort();
    }

    public void WaitForWrites() {
      if (Paused || !Enabled) {
        throw new InvalidOperationException("Requested to wait while not enabled or paused");
      }
      m_writePending.Set();
      m_writeQueueCleared.Reset();
      m_writeQueueCleared.WaitOne();
    }

    public void WaitForReads() {
      if (Paused || !Enabled) {
        throw new InvalidOperationException("Requested to wait while not enabled or paused");
      }
      m_readQueueCleared.Reset();
      m_readQueueCleared.WaitOne();
    }

    public void AsyncWrite(Action action) {
      lock (m_writeQueue) {
        m_writeQueue.Add(action);
        // Signal the writer thread that there is work to be done.
        m_writePending.Set();
      }
    }

    public void AsyncRead(Action action) {
      lock (m_readQueue) {
        m_readQueue.Add(action);
        // Signal the reader thread that there is work to be done.
        m_readPending.Set();
      }
    }

    private void RunReaderLoop() {
      List<Action> localQueue = new List<Action>();
      WorkDispatcher dispatch = new WorkDispatcher();
      dispatch.queue = localQueue;

      while (true) {
        if (!Enabled) { return; }
        if (Paused) {
          Thread.Sleep(100);
          continue;
        }

        // Wait for a work item to be queued.
        m_readPending.WaitOne();

        lock (m_readQueue) {
          if (m_readQueue.Count > 0) {
            localQueue.AddRange(m_readQueue);
            m_readQueue.Clear();
          }
        }

        if (localQueue.Count == 0) {
          continue;
        }

        // Run C# code via TBB, which has significantly better load balancing than ThreadPool.
        //
        // TODO: once Unity supports a modern version of C#, the native task system would probably
        // be preferable to avoid the trip through C++.

        // Process all work in parallel, blocking until complete.
        pxr.UsdCs.ParallelForN((uint)localQueue.Count, dispatch);
        localQueue.Clear();

        foreach (Action action in localQueue) {
          action();
        }
        localQueue.Clear();

        // Signal that we are done reading.
        m_readQueueCleared.Set();
      }
    }

    private void RunWriterLoop() {
      List<Action> localQueue = new List<Action>();
      WorkDispatcher dispatch = new WorkDispatcher();
      dispatch.queue = localQueue;

      while (true) {
        if (!Enabled) { return; }
        if (Paused) {
          Thread.Sleep(100);
          continue;
        }

        // Wait for an item to be queued.
        m_writePending.WaitOne();

        lock (m_writeQueue) {
          if (m_writeQueue.Count > 0) {
            localQueue.AddRange(m_writeQueue);
            m_writeQueue.Clear();
          }
        }

        // Run C# code via TBB, which has significantly better load balancing than ThreadPool.
        //
        // TODO: once Unity supports a modern version of C#, the native task system would probably
        // be preferable to avoid the trip through C++.

        // Disabled because this is likely unsafe for garbage collection -- TODO: research this.

        // Process all work in parallel, blocking until complete.
        //pxr.UsdCs.ParallelForN((uint)localQueue.Count, dispatch);
        //localQueue.Clear();

        foreach (Action action in localQueue) {
          action();
        }
        localQueue.Clear();

        // Signal that we are done writing.
        m_writeQueueCleared.Set();
      }
    }

    private class WorkDispatcher : pxr.TaskCallback {
      public List<Action> queue;

      public override void Run(uint startIndex, uint endIndex) {
        for (uint i = startIndex; i < endIndex; i++) {
          queue[(int)i]();
        }
      }
    }

    private Thread m_reader;
    private List<Action> m_readQueue = new List<Action>();
    private AutoResetEvent m_readPending = new AutoResetEvent(false);
    private AutoResetEvent m_readQueueCleared = new AutoResetEvent(false);

    private Thread m_writer;
    private List<Action> m_writeQueue = new List<Action>();
    private AutoResetEvent m_writePending = new AutoResetEvent(false);
    private AutoResetEvent m_writeQueueCleared = new AutoResetEvent(false);
  }
}
