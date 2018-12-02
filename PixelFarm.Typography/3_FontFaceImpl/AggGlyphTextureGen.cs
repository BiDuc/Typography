﻿//MIT, 2016-present, WinterDev
//-----------------------------------  
using System;
using PixelFarm.CpuBlit.VertexProcessing;

using PixelFarm.Drawing;
using PixelFarm.CpuBlit;
using PixelFarm.Drawing.Fonts;

using Typography.Rendering;
namespace Typography.Contours
{
    /// <summary>
    /// agg glyph texture generator
    /// </summary>
    public class AggGlyphTextureGen
    {
        GlyphTranslatorToVxs _txToVxs = new GlyphTranslatorToVxs();

        public AggGlyphTextureGen()
        {
            BackGroundColor = Color.Transparent;
            GlyphColor = Color.Black;
        }

        public Color BackGroundColor { get; set; }
        public Color GlyphColor { get; set; }
        public TextureKind TextureKind { get; set; }
        public AggPainter Painter { get; set; }
        public GlyphImage CreateGlyphImage(GlyphPathBuilder builder, float pxscale)
        {

            _txToVxs.Reset();
            //1. builder read shape and translate it with _txToVxs
            builder.ReadShapes(_txToVxs);

            using (VxsTemp.Borrow(out var glyphVxs, out var vxs2))
            {
                //2. write translated data (in the _txToVxs) to glyphVxs

                _txToVxs.WriteOutput(glyphVxs, pxscale);

                RectD bounds = glyphVxs.GetBoundingRect();

                //-------------------------------------------- 
                int w = (int)System.Math.Ceiling(bounds.Width);
                int h = (int)System.Math.Ceiling(bounds.Height);
                if (w < 5)
                {
                    w = 5;
                }
                if (h < 5)
                {
                    h = 5;
                }


                //we need some margin
                int horizontal_margin = 1;
                int vertical_margin = 1;


                //translate to positive quadrant and use minimum space

                double dx = Math.Ceiling((bounds.Left < 0) ? -bounds.Left : 0);
                double dy = 0;

                //vertical adjust =>since we need to move it, then move it with integer value
                if (bounds.Bottom < 0)
                {
                    dy = Math.Ceiling(-bounds.Bottom);
                }
                else if (bounds.Bottom > 0)
                {
                    dy = Math.Floor(-bounds.Bottom);
                }
                dx += horizontal_margin;
                dy += vertical_margin;
                //--------------------------------------------  

                w = (int)Math.Ceiling(dx + w + horizontal_margin); //+right margin
                h = (int)Math.Ceiling((double)(vertical_margin + h + vertical_margin)); //+bottom margin 



                AggPainter painter = Painter;
                if (TextureKind == TextureKind.StencilLcdEffect)
                {

                    glyphVxs.TranslateToNewVxs(dx + 0.33f, dy, vxs2); //offset to proper x of subpixel rendering  ***
                    glyphVxs = vxs2;
                    // 
                    painter.UseSubPixelLcdEffect = true;
                    //we use white glyph on black bg for this texture                
                    painter.Clear(Color.Black);
                    painter.FillColor = Color.White;
                    painter.Fill(glyphVxs);

                    //apply sharpen filter
                    //painter.DoFilter(new RectInt(0, h, w, 0), 2);
                    //painter.DoFilter(new RectInt(0, h, w, 0), 2); //? 
                }
                else
                {

                    glyphVxs.TranslateToNewVxs(dx, dy, vxs2);
                    glyphVxs = vxs2;

                    painter.UseSubPixelLcdEffect = false;

                    if (TextureKind == TextureKind.StencilGreyScale)
                    {
                        painter.Clear(Color.Empty);
                        painter.FillColor = Color.Black;
                    }
                    else
                    {
                        painter.Clear(BackGroundColor);
                        painter.FillColor = this.GlyphColor;
                    }
                    painter.Fill(glyphVxs);

                }
                //
                var glyphImage = new GlyphImage(w, h);
                glyphImage.TextureOffsetX = dx;
                glyphImage.TextureOffsetY = dy;
                glyphImage.SetImageBuffer(MemBitmapExtensions.CopyImgBuffer(painter.RenderSurface.DestBitmap, w, h), false);
                //copy data from agg canvas to glyph image 
                return glyphImage;

            }

        }
    }

}