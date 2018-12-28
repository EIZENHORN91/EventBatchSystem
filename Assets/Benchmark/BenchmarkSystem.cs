using BovineLabs.Common.Utility;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;

public class EventBatchSystemBenchmark : JobComponentSystem
{
    private EventBatchSystem eventBatchSystem;

    /// <inheritdoc />
    protected override void OnCreateManager()
    {
        this.eventBatchSystem = this.World.GetOrCreateManager<EventBatchSystem>();
    }

    protected override JobHandle OnUpdate(JobHandle handle)
    {
        handle =  new Job
            {
                Queue11 = this.eventBatchSystem.GetEventBatch<Test1>(this).ToConcurrent(),
                Queue12 = this.eventBatchSystem.GetEventBatch<Test1>(this).ToConcurrent(),
                Queue2 = this.eventBatchSystem.GetEventBatch<Test2>(this).ToConcurrent(),
            }
            .Schedule(333333, 1024, handle);

        return handle;
    }

    [BurstCompile]
    private struct Job : IJobParallelFor
    {
        public NativeQueue<Test1>.Concurrent Queue11;
        public NativeQueue<Test1>.Concurrent Queue12;
        public NativeQueue<Test2>.Concurrent Queue2;

        /// <inheritdoc />
        public void Execute(int index)
        {
            this.Queue11.Enqueue(new Test1 { Value = index });
            this.Queue12.Enqueue(new Test1 { Value = -index });
            this.Queue2.Enqueue(new Test2 { Value = index });
        }
    }

    private struct Test1 : IComponentData
    {
        public int Value;
    }

    private struct Test2 : IComponentData
    {
        public int Value;
    }
}

[AlwaysUpdateSystem]
public class EndFrameBarrierSystemBenchmark : JobComponentSystem
{
    [Inject] private EndFrameBarrier endFrameBarrier;

    protected override JobHandle OnUpdate(JobHandle handle)
    {
        return handle;

        handle = new Destroy
            {
                EntityCommandBuffer = this.endFrameBarrier.CreateCommandBuffer().ToConcurrent()
            }
            .Schedule(this, handle);

        handle = new Destroy2
            {
                EntityCommandBuffer = this.endFrameBarrier.CreateCommandBuffer().ToConcurrent()
            }
            .Schedule(this, handle);

        handle = new Job
            {
                EntityCommandBuffer = this.endFrameBarrier.CreateCommandBuffer().ToConcurrent()
            }
            .Schedule(33333, 1024, handle);

        handle.Complete();

        return handle;
    }

    private struct Destroy : IJobProcessComponentDataWithEntity<Test1>
    {
        public EntityCommandBuffer.Concurrent EntityCommandBuffer;

        /// <inheritdoc />
        public void Execute(Entity entity, int index, [ReadOnly] ref Test1 data)
        {
            this.EntityCommandBuffer.DestroyEntity(index, entity);
        }
    }

    private struct Destroy2 : IJobProcessComponentDataWithEntity<Test2>
    {
        public EntityCommandBuffer.Concurrent EntityCommandBuffer;

        /// <inheritdoc />
        public void Execute(Entity entity, int index, [ReadOnly] ref Test2 data)
        {
            this.EntityCommandBuffer.DestroyEntity(index, entity);
        }
    }

    private struct Job : IJobParallelFor
    {
        public EntityCommandBuffer.Concurrent EntityCommandBuffer;

        /// <inheritdoc />
        public void Execute(int index)
        {
            this.EntityCommandBuffer.CreateEntity(index);
            this.EntityCommandBuffer.AddComponent(index, new Test1 { Value = index });

            this.EntityCommandBuffer.CreateEntity(index);
            this.EntityCommandBuffer.AddComponent(index, new Test1 { Value = -index });

            this.EntityCommandBuffer.CreateEntity(index);
            this.EntityCommandBuffer.AddComponent(index, new Test2 { Value = index });
        }
    }

    private struct Test1 : IComponentData
    {
        public int Value;
    }

    private struct Test2 : IComponentData
    {
        public int Value;
    }
}

/*public class EventBatchSystemBenchmark : JobComponentSystem
{
    private EventBatchSystem eventBatchSystem;

    /// <inheritdoc />
    protected override void OnCreateManager()
    {
        this.eventBatchSystem = this.World.GetOrCreateManager<EventBatchSystem>();
    }

    protected override JobHandle OnUpdate(JobHandle handle)
    {
        return new Job
            {
                Queue11 = this.eventBatchSystem.GetEventBatch<Test1>().ToConcurrent(),
                Queue12 = this.eventBatchSystem.GetEventBatch<Test1>().ToConcurrent(),
                Queue2 = this.eventBatchSystem.GetEventBatch<Test2>().ToConcurrent(),
            }
            .Schedule(100000, 1024, handle);
    }

    [BurstCompile]
    private struct Job : IJobParallelFor
    {
        public NativeQueue<Test1>.Concurrent Queue11;
        public NativeQueue<Test1>.Concurrent Queue12;

        public NativeQueue<Test2>.Concurrent Queue2;

        /// <inheritdoc />
        public void Execute(int index)
        {
            this.Queue11.Enqueue(new Test1 { Value = index });
            this.Queue12.Enqueue(new Test1 { Value = -index });
            this.Queue2.Enqueue(new Test2 { Value = index });
        }
    }

    private struct Test1 : IComponentData
    {
        public int Value;
    }

    private struct Test2 : IComponentData
    {
        public int Value;
    }
}*/
