using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows;
using System.Windows.Forms;
using Triangulation;

namespace TriangulatorSample
{
    /// <summary>
    /// A quick and dirty sample to show what the triangulator does.
    /// </summary>
    class SampleApp
    {
        static void Main(string[] args)
        {
            Form foo = new Form();
            foo.Top = 0;
            foo.Left = 0;
            foo.Width = 450;
            foo.Height = 900;
            foo.BackColor = Color.SlateGray;

            foo.Shown += Foo_Shown;

            Application.Run(foo);
        }

        private static void Foo_Shown(object sender, EventArgs e)
        {
            var foo = (Form)sender;
            Pen bar = new Pen(Color.LimeGreen);
            Graphics qux = foo.CreateGraphics();

            Vector[] V = new Vector[]
            {
                new Vector(100, 200),
                new Vector(200, 100),
                new Vector(400, 150),
                new Vector(300, 200),
                new Vector(350, 300),
                new Vector(150, 400),
                new Vector(250, 300)
            };

            // Draw input polygon
            DrawPolygon(qux, bar, V, -25, -50);

            // *** This line here is how you use Triangulator ***
            IList<int> triangles = Triangulator.Triangulate(V).ToList();

            // Draw triangles produced by Triangulator
            while (triangles.Count() >= 3)
            {
                DrawPolygon(qux, bar, new Vector[]{ V[triangles[0]], V[triangles[1]], V[triangles[2]] }, -25, 400);

                triangles.RemoveAt(0);
                triangles.RemoveAt(0);
                triangles.RemoveAt(0);
            }
        }

        private static void DrawPolygon(Graphics graphics, Pen pen, IEnumerable<Vector> polygon, int xOffset, int yOffset)
        {
            graphics.DrawPolygon(pen, polygon.Select(v => new PointF((float)v.X + xOffset, (float)v.Y + yOffset)).ToArray());
        }
    }
}
