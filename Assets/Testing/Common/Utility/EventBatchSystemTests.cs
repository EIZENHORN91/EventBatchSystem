﻿// <copyright file="BatchSystemTests.cs" company="Timothy Raines">
//     Copyright (c) Timothy Raines. All rights reserved.
// </copyright>

namespace BovineLabs.Testing.Common.Utility
{
    using BovineLabs.Common.Utility;
    using NUnit.Framework;
    using Unity.Collections;
    using Unity.Entities;
    using Unity.Entities.Tests;

    /// <summary>
    /// The BatchSystemTests.
    /// </summary>
    public class BatchSystemTests : ECSTestsFixture
    {
        /// <summary>
        /// 
        /// </summary>
        [Test]
        public void GetEventBatchSingleBatchCreates()
        {
            var batchSystem = this.World.CreateManager<EventBatchSystem>();

            var queue = batchSystem.GetEventBatch<TestData>(this.EmptySystem);

            var data0 = new TestData { Value = 0 };
            var data1 = new TestData { Value = 1 };
            var data2 = new TestData { Value = 2 };

            queue.Enqueue(data0);
            queue.Enqueue(data1);
            queue.Enqueue(data2);

            batchSystem.Update();

            var group = this.m_Manager.CreateComponentGroup(typeof(TestData));
            Assert.AreEqual(3, group.CalculateLength());

            var chunks = group.CreateArchetypeChunkArray(Allocator.TempJob);

            int count = 0;

            var testDataType = this.m_Manager.GetArchetypeChunkComponentType<TestData>(true);

            for (var chunkIndex = 0; chunkIndex < chunks.Length; chunkIndex++)
            {
                var chunk = chunks[chunkIndex];
                var data = chunk.GetNativeArray(testDataType);

                for (var index = 0; index < chunk.Count; index++)
                {
                    Assert.AreEqual(count++, data[index].Value);
                }
            }

            chunks.Dispose();
        }

        [Test]
        public void GetEventBatchReturnsDifferentQueues()
        {
            var batchSystem = this.World.CreateManager<EventBatchSystem>();

            Assert.AreNotEqual(batchSystem.GetEventBatch<TestData>(this.EmptySystem), batchSystem.GetEventBatch<TestData>(this.EmptySystem));
        }

        [Test]
        public void GetEventBatchMultipleQueueWork()
        {
            var batchSystem = this.World.CreateManager<EventBatchSystem>();

            var queue1 = batchSystem.GetEventBatch<TestData>(this.EmptySystem);
            var queue2 = batchSystem.GetEventBatch<TestData>(this.EmptySystem);

            var data0 = new TestData { Value = 0 };
            var data1 = new TestData { Value = 1 };
            var data2 = new TestData { Value = 2 };

            queue1.Enqueue(data0);
            queue1.Enqueue(data1);
            queue2.Enqueue(data2);

            batchSystem.Update();

            var group = this.m_Manager.CreateComponentGroup(typeof(TestData));
            Assert.AreEqual(3, group.CalculateLength());

            var chunks = group.CreateArchetypeChunkArray(Allocator.TempJob);

            int count = 0;

            var testDataType = this.m_Manager.GetArchetypeChunkComponentType<TestData>(true);

            for (var chunkIndex = 0; chunkIndex < chunks.Length; chunkIndex++)
            {
                var chunk = chunks[chunkIndex];
                var data = chunk.GetNativeArray(testDataType);

                for (var index = 0; index < chunk.Count; index++)
                {
                    var value = data[index].Value;
                    Assert.AreEqual(count++, value);
                }
            }

            chunks.Dispose();
        }

        [Test]
        public void GetEventBatchMultipleTypesCreates()
        {
            var batchSystem = this.World.CreateManager<EventBatchSystem>();

            var queue = batchSystem.GetEventBatch<TestData>(this.EmptySystem);
            var queue2 = batchSystem.GetEventBatch<TestData2>(this.EmptySystem);

            var data0 = new TestData { Value = 0 };
            var data1 = new TestData { Value = 1 };
            var data2 = new TestData { Value = 2 };

            var data20 = new TestData2 { Value = 0 };
            var data21 = new TestData2 { Value = 1 };
            var data22 = new TestData2 { Value = 2 };

            queue.Enqueue(data0);
            queue.Enqueue(data1);
            queue.Enqueue(data2);

            queue2.Enqueue(data20);
            queue2.Enqueue(data21);
            queue2.Enqueue(data22);

            batchSystem.Update();

            var group = this.m_Manager.CreateComponentGroup(typeof(TestData));
            Assert.AreEqual(3, group.CalculateLength());

            var group2 = this.m_Manager.CreateComponentGroup(typeof(TestData2));
            Assert.AreEqual(3, group2.CalculateLength());

            var chunks = group.CreateArchetypeChunkArray(Allocator.TempJob);

            int count = 0;

            var testDataType = this.m_Manager.GetArchetypeChunkComponentType<TestData>(true);

            for (var chunkIndex = 0; chunkIndex < chunks.Length; chunkIndex++)
            {
                var chunk = chunks[chunkIndex];
                var data = chunk.GetNativeArray(testDataType);

                for (var index = 0; index < chunk.Count; index++)
                {
                    Assert.AreEqual(count++, data[index].Value);
                }
            }

            chunks.Dispose();

            var chunks2 = group2.CreateArchetypeChunkArray(Allocator.TempJob);

            int count2 = 0;

            var testData2Type = this.m_Manager.GetArchetypeChunkComponentType<TestData2>(true);

            for (var chunkIndex = 0; chunkIndex < chunks2.Length; chunkIndex++)
            {
                var chunk = chunks2[chunkIndex];
                var data = chunk.GetNativeArray(testData2Type);

                for (var index = 0; index < chunk.Count; index++)
                {
                    Assert.AreEqual(count2++, data[index].Value);
                }
            }

            chunks2.Dispose();
        }

        private struct TestData : IComponentData
        {
            public int Value;
        }

        private struct TestData2 : IComponentData
        {
            public int Value;
        }
    }
}