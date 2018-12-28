﻿// <copyright file="NativeUnit.cs" company="Timothy Raines">
//     Copyright (c) Timothy Raines. All rights reserved.
// </copyright>

namespace BovineLabs.Common.Native
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using Unity.Burst;
    using Unity.Collections;
    using Unity.Collections.LowLevel.Unsafe;

    /// <summary>
    /// A single value native container to allow values to be passed between jobs.
    /// </summary>
    /// <typeparam name="T">The type of the <see cref="NativeUnit{T}"/>.</typeparam>
    [NativeContainerSupportsDeallocateOnJobCompletion]
    [NativeContainer]
    public unsafe struct NativeUnit<T> : IDisposable
        where T : struct
    {
#pragma warning disable SA1308 // Variable names should not be prefixed
        [NativeDisableUnsafePtrRestriction]
        [SuppressMessage("ReSharper", "InconsistentNaming", Justification = "Used implicity by ECS")]
        private void* m_Buffer;

        [SuppressMessage("ReSharper", "InconsistentNaming", Justification = "Used implicity by ECS")]
        [SuppressMessage("ReSharper", "FieldCanBeMadeReadOnly.Local", Justification = "Used implicity by ECS")]
        private Allocator m_AllocatorLabel;

#if ENABLE_UNITY_COLLECTIONS_CHECKS
        [SuppressMessage("ReSharper", "InconsistentNaming", Justification = "Used implicity by ECS")]
        private AtomicSafetyHandle m_Safety;

        [NativeSetClassTypeToNullOnSchedule]
        [SuppressMessage("ReSharper", "InconsistentNaming", Justification = "Used implicity by ECS")]
        private DisposeSentinel m_DisposeSentinel;
#pragma warning restore SA1308 // Variable names should not be prefixed
#endif

        /// <summary>
        /// Initializes a new instance of the <see cref="NativeUnit{T}"/> struct.
        /// </summary>
        /// <param name="allocator">The <see cref="Allocator"/> of the <see cref="NativeUnit{T}"/>.</param>
        /// <param name="options">The default memory state.</param>
        public NativeUnit(Allocator allocator, NativeArrayOptions options = NativeArrayOptions.ClearMemory)
        {
            if (allocator <= Allocator.None)
            {
                throw new ArgumentException("Allocator must be Temp, TempJob or Persistent", nameof(allocator));
            }

            IsBlittableAndThrow();

            var size = UnsafeUtility.SizeOf<T>();
            this.m_Buffer = UnsafeUtility.Malloc(size, UnsafeUtility.AlignOf<T>(), allocator);
            this.m_AllocatorLabel = allocator;

#if ENABLE_UNITY_COLLECTIONS_CHECKS
            DisposeSentinel.Create(out this.m_Safety, out this.m_DisposeSentinel, 1, allocator);
#endif

            if ((options & NativeArrayOptions.ClearMemory) == NativeArrayOptions.ClearMemory)
            {
                UnsafeUtility.MemClear(this.m_Buffer, UnsafeUtility.SizeOf<T>());
            }
        }

        /// <summary>
        /// Gets or sets the value of the unit.
        /// </summary>
        public T Value
        {
            get
            {
#if ENABLE_UNITY_COLLECTIONS_CHECKS
                AtomicSafetyHandle.CheckReadAndThrow(this.m_Safety);
#endif
                return UnsafeUtility.ReadArrayElement<T>(this.m_Buffer, 0);
            }

            [WriteAccessRequired]
            set
            {
#if ENABLE_UNITY_COLLECTIONS_CHECKS
                AtomicSafetyHandle.CheckWriteAndThrow(this.m_Safety);
#endif
                UnsafeUtility.WriteArrayElement(this.m_Buffer, 0, value);
            }
        }

        /// <summary>
        /// Gets a value indicating whether the <see cref="NativeUnit{T}"/> has been initialized.
        /// </summary>
        public bool IsCreated => (IntPtr)this.m_Buffer != IntPtr.Zero;

        /// <inheritdoc/>
        [WriteAccessRequired]
        public void Dispose()
        {
            if (!UnsafeUtility.IsValidAllocator(this.m_AllocatorLabel))
            {
                throw new InvalidOperationException(
                    "The NativeArray can not be Disposed because it was not allocated with a valid allocator.");
            }

#if ENABLE_UNITY_COLLECTIONS_CHECKS
            DisposeSentinel.Dispose(ref this.m_Safety, ref this.m_DisposeSentinel);
#endif
            UnsafeUtility.Free(this.m_Buffer, this.m_AllocatorLabel);
            this.m_Buffer = null;
        }

        [BurstDiscard]
        private static void IsBlittableAndThrow()
        {
            if (!UnsafeUtility.IsBlittable<T>())
            {
                throw new ArgumentException($"{typeof(T)} used in NativeArray<{typeof(T)}> must be blittable");
            }
        }
    }
}