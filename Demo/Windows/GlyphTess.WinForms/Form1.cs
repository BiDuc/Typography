﻿//MIT, 2017-present, WinterDev
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

//
using Typography.OpenFont;
//
using DrawingGL;
using DrawingGL.Text;
//
using Tesselate;

namespace Test_WinForm_TessGlyph
{
    public partial class FormTess : Form
    {
        Graphics _g;
        float[] _glyphPoints2;
        int[] _contourEnds;

        TessTool _tessTool = new TessTool();
        public FormTess()
        {
            InitializeComponent();
            this.textBox1.KeyUp += TextBox1_KeyUp;
        }

        private void TextBox1_KeyUp(object sender, KeyEventArgs e)
        {
            string oneChar = this.textBox1.Text.Trim();
            if (string.IsNullOrEmpty(oneChar)) return;
            //
            char selectedChar = oneChar[0];
            //
            //
            //selectedChar = 'e'; 
            if (_g == null)
            {
                _g = this.pnlGlyph.CreateGraphics();
            }
            _g.Clear(Color.White);



            //string testFont = "d:\\WImageTest\\DroidSans.ttf";
            //string testFont = "c:\\Windows\\Fonts\\Tahoma.ttf";
            string testFont = "d:\\WImageTest\\Alfa_Slab.ttf";

            using (FileStream fs = new FileStream(testFont, FileMode.Open, FileAccess.Read))
            {
                OpenFontReader reader = new OpenFontReader();
                Typeface typeface = reader.Read(fs);

                //--
                var builder = new Typography.Contours.GlyphPathBuilder(typeface);
                builder.BuildFromGlyphIndex(typeface.LookupIndex(selectedChar), 300);

                var txToPath = new GlyphTranslatorToPath();
                var writablePath = new WritablePath();
                txToPath.SetOutput(writablePath);
                builder.ReadShapes(txToPath);
                //from contour to  
                var curveFlattener = new SimpleCurveFlattener();
                float[] flattenPoints = curveFlattener.Flatten(writablePath._points, out _contourEnds);
                _glyphPoints2 = flattenPoints;
                ////--------------------------------------
                ////raw glyph points
                //int j = glyphPoints.Length;
                //float scale = typeface.CalculateToPixelScaleFromPointSize(256);
                //glyphPoints2 = new float[j * 2];
                //int n = 0;
                //for (int i = 0; i < j; ++i)
                //{
                //    GlyphPointF pp = glyphPoints[i];
                //    glyphPoints2[n] = pp.X * scale;
                //    n++;
                //    glyphPoints2[n] = pp.Y * scale;
                //    n++;
                //}
                ////--------------------------------------
            }
            DrawOutput();
        }
        private void FormTess_Load(object sender, EventArgs e)
        {
            if (_g == null)
            {
                _g = this.pnlGlyph.CreateGraphics();
            }
            _g.Clear(Color.White);

        }

        float[] GetPolygonData(out int[] endContours)
        {
            endContours = _contourEnds;
            return _glyphPoints2;

            ////--
            ////for test

            //return new float[]
            //{
            //        10,10,
            //        200,10,
            //        100,100,
            //        150,200,
            //        20,200,
            //        50,100
            //};
        }


        float[] TransformPoints(float[] polygonXYs, System.Drawing.Drawing2D.Matrix transformMat)
        {
            //for example only

            PointF[] points = new PointF[1];
            float[] transformXYs = new float[polygonXYs.Length];

            for (int i = 0; i < polygonXYs.Length;)
            {
                points[0] = new PointF(polygonXYs[i], polygonXYs[i + 1]);
                transformMat.TransformPoints(points);

                transformXYs[i] = points[0].X;
                transformXYs[i + 1] = points[0].Y;
                i += 2;
            }
            return transformXYs;
        }
        void DrawOutput()
        {
            if (_g == null)
            {
                return;
            }

            //-----------
            //for GDI+ only
            bool drawInvert = chkInvert.Checked;
            int viewHeight = this.pnlGlyph.Height;

            //----------- 
            //show tess
            _g.Clear(Color.White);
            int[] contourEndIndices;
            float[] polygon1 = GetPolygonData(out contourEndIndices);
            if (polygon1 == null) return;
            //
            if (drawInvert)
            {
                var transformMat = new System.Drawing.Drawing2D.Matrix();
                transformMat.Scale(1, -1);
                transformMat.Translate(0, -viewHeight);
                //
                polygon1 = TransformPoints(polygon1, transformMat);
            }


            using (Pen pen1 = new Pen(Color.LightGray, 6))
            {
                int nn = polygon1.Length;
                int a = 0;
                PointF p0;
                PointF p1;

                int contourCount = contourEndIndices.Length;
                int startAt = 3;
                for (int cnt_index = 0; cnt_index < contourCount; ++cnt_index)
                {
                    int endAt = contourEndIndices[cnt_index];
                    for (int m = startAt; m <= endAt;)
                    {
                        p0 = new PointF(polygon1[m - 3], polygon1[m - 2]);
                        p1 = new PointF(polygon1[m - 1], polygon1[m]);
                        _g.DrawLine(pen1, p0, p1);
                        _g.DrawString(a.ToString(), this.Font, Brushes.Black, p0);
                        m += 2;
                        a++;
                    }
                    //close contour 

                    p0 = new PointF(polygon1[endAt - 1], polygon1[endAt]);
                    p1 = new PointF(polygon1[startAt - 3], polygon1[startAt - 2]);
                    _g.DrawLine(pen1, p0, p1);
                    _g.DrawString(a.ToString(), this.Font, Brushes.Black, p0);
                    //
                    startAt = (endAt + 1) + 3;
                }
            }

            if (!_tessTool.TessPolygon(polygon1, _contourEnds))
            {
                return;
            }

            //1.
            List<ushort> indexList = _tessTool.TessIndexList;
            //2.
            List<TessVertex2d> tempVertexList = _tessTool.TempVertexList;
            //3.
            int vertexCount = indexList.Count;
            //-----------------------------    
            int orgVertexCount = polygon1.Length / 2;
            float[] vtx = new float[vertexCount * 2];//***
            int n = 0;

            for (int p = 0; p < vertexCount; ++p)
            {
                ushort index = indexList[p];
                if (index >= orgVertexCount)
                {
                    //extra coord (newly created)
                    TessVertex2d extraVertex = tempVertexList[index - orgVertexCount];
                    vtx[n] = (float)extraVertex.x;
                    vtx[n + 1] = (float)extraVertex.y;
                }
                else
                {
                    //original corrd
                    vtx[n] = (float)polygon1[index * 2];
                    vtx[n + 1] = (float)polygon1[(index * 2) + 1];
                }
                n += 2;
            }
            //-----------------------------    
            //draw tess result
            int j = vtx.Length;
            for (int i = 0; i < j;)
            {
                var p0 = new PointF(vtx[i], vtx[i + 1]);
                var p1 = new PointF(vtx[i + 2], vtx[i + 3]);
                var p2 = new PointF(vtx[i + 4], vtx[i + 5]);

                _g.DrawLine(Pens.Red, p0, p1);
                _g.DrawLine(Pens.Red, p1, p2);
                _g.DrawLine(Pens.Red, p2, p0);

                i += 6;
            }

        }
        private void cmdDrawGlyph_Click(object sender, EventArgs e)
        {
            DrawOutput();
        }

        private void chkInvert_CheckedChanged(object sender, EventArgs e)
        {
            DrawOutput();
        }
    }
}
