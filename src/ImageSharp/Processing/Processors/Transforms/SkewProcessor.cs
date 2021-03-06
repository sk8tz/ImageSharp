﻿// Copyright (c) Six Labors and contributors.
// Licensed under the Apache License, Version 2.0.

using System;
using System.Numerics;
using System.Threading.Tasks;
using SixLabors.ImageSharp.Advanced;
using SixLabors.ImageSharp.Helpers;
using SixLabors.ImageSharp.Memory;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.Primitives;

namespace SixLabors.ImageSharp.Processing.Processors
{
    /// <summary>
    /// Provides methods that allow the skewing of images.
    /// </summary>
    /// <typeparam name="TPixel">The pixel format.</typeparam>
    internal class SkewProcessor<TPixel> : Matrix3x2Processor<TPixel>
        where TPixel : struct, IPixel<TPixel>
    {
        /// <summary>
        /// The transform matrix to apply.
        /// </summary>
        private Matrix3x2 processMatrix;

        /// <summary>
        /// Gets or sets the angle of rotation along the x-axis in degrees.
        /// </summary>
        public float AngleX { get; set; }

        /// <summary>
        /// Gets or sets the angle of rotation along the y-axis in degrees.
        /// </summary>
        public float AngleY { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to expand the canvas to fit the skewed image.
        /// </summary>
        public bool Expand { get; set; } = true;

        /// <inheritdoc/>
        protected override void OnApply(ImageFrame<TPixel> source, Rectangle sourceRectangle, Configuration configuration)
        {
            int height = this.CanvasRectangle.Height;
            int width = this.CanvasRectangle.Width;
            Matrix3x2 matrix = this.GetCenteredMatrix(source, this.processMatrix);
            Rectangle sourceBounds = source.Bounds();

            using (var targetPixels = new PixelAccessor<TPixel>(width, height))
            {
                Parallel.For(
                    0,
                    height,
                    configuration.ParallelOptions,
                    y =>
                        {
                            Span<TPixel> targetRow = targetPixels.GetRowSpan(y);

                            for (int x = 0; x < width; x++)
                            {
                                var transformedPoint = Point.Skew(new Point(x, y), matrix);

                                if (sourceBounds.Contains(transformedPoint.X, transformedPoint.Y))
                                {
                                    targetRow[x] = source[transformedPoint.X, transformedPoint.Y];
                                }
                            }
                        });

                source.SwapPixelsBuffers(targetPixels);
            }
        }

        /// <inheritdoc/>
        protected override void BeforeApply(ImageFrame<TPixel> source, Rectangle sourceRectangle, Configuration configuration)
        {
            this.processMatrix = Matrix3x2Extensions.CreateSkewDegrees(-this.AngleX, -this.AngleY, new Point(0, 0));
            if (this.Expand)
            {
                this.CreateNewCanvas(sourceRectangle, this.processMatrix);
            }
        }
    }
}