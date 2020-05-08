using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Pooop
{
    public partial class Form1 : Form
    {
        PointF mouse = new PointF();
        World world;
        int frame = 0;
        float time = 0.0f;
        bool drag = false;
        bool view = false;

        public Form1()
        {
            InitializeComponent();
            this.pictureBox1.MouseWheel +=
                new System.Windows.Forms.MouseEventHandler(this.pictureBox1_MouseWheel);
            world = new World(
                new PointF(pictureBox1.Width, pictureBox1.Height)
            );
            timer1.Enabled = true;
        }

        private void PictureBox1_Paint(object sender, PaintEventArgs e)
        {
            e.Graphics.FillRectangle(new SolidBrush(Color.FromArgb(240, 240, 240)), pictureBox1.ClientRectangle);
            if (drag) world.Target(mouse);
            world.Update();
            world.Render(e.Graphics);

            frame++;
            time += (float)timer1.Interval / 1000.0f;
        }

        private void Timer1_Tick(object sender, EventArgs e)
        {
            pictureBox1.Invalidate();
        }

        private void PictureBox1_MouseMove(object sender, MouseEventArgs e)
        {
            mouse = (PointF)e.Location;
            world.Mouse(mouse);
        }

        private void pictureBox1_MouseWheel(object sender, MouseEventArgs e)
        {
            world.ball.Radius += (float)e.Delta * 0.1f;
            world.ball.Radius = Math.Max(0.0f, Math.Min(100.0f, world.ball.Radius));
        }

        private void PictureBox1_MouseDown(object sender, MouseEventArgs e)
        {
            switch (e.Button)
            {
                case MouseButtons.Left:
                    drag = true;
                    break;
            }
        }
        private void PictureBox1_MouseUp(object sender, MouseEventArgs e)
        {
            switch (e.Button)
            {
                case MouseButtons.Left:
                    drag = false;
                    break;
            }
        }

        private void Form1_KeyDown(object sender, KeyEventArgs e)
        {
            switch (e.KeyData)
            {
                case Keys.Space:
                    world.view = !world.view;
                    break;
                case Keys.G:
                    world.gr = !world.gr;
                    break;
                case Keys.R:
                    world.r = !world.r;
                    break;
            }
        }

        private void PictureBox1_Click(object sender, EventArgs e) {
            
        }
    }
    public class World
    {
        private List<RectObject> rects = new List<RectObject>();
        private List<RectObject> blocks = new List<RectObject>();
        private PointF target = new PointF();
        private PointF mouse = new PointF();
        private PointF size = new PointF();
        public EllipseObject ball;
        private bool enableTarget = false;
        private int frame = 0;
        public bool view = false;
        public bool gr = false;
        public bool r = false;
        public World(PointF size)
        {
            this.size = size;
            AddRectObject(new RectObject(
                new PointF(
                    -100.0f, size.Y * 0.5f
                ),
                new PointF(
                    200.0f, size.Y
                )
            ));
            AddRectObject(new RectObject(
                new PointF(
                    size.X + 100.0f, size.Y * 0.5f
                ),
                new PointF(
                    200.0f, size.Y
                )
            ));
            AddRectObject(new RectObject(
                new PointF(
                    size.X * 0.5f, -100.0f
                ),
                new PointF(
                    size.X, 200.0f
                )
            ));
            AddRectObject(new RectObject(
                new PointF(
                    size.X * 0.5f, size.Y + 100.0f
                ),
                new PointF(
                    size.X, 200.0f
                )
            ));
            RectObject block = new RectObject(
                new PointF(size.X / 2.0f, size.Y / 2.0f),
                new PointF(300.0f, 100.0f),
                0.0f, 0.4f
            );
            blocks.Add(block);
            block.targetRadian += 1.0f;
            AddRectObject(block);
            ball = new EllipseObject(new PointF(size.X / 2.0f, size.Y / 8.0f), 0.0f);
            enableTarget = false;
        }
        public void AddRectObject(RectObject rect) { rects.Add(rect); }
        public void Target(PointF target)
        {
            enableTarget = true;
            this.target = target;
        }
        public void Mouse(PointF mouse)
        {
            this.mouse = mouse;
        }
        public void Update()
        {
            frame++;
            foreach (RectObject rect in rects) rect.Update();
            if (!r) foreach (RectObject block in blocks) block.targetRadian += 0.01f;
            ball.Update();
            if (ball.Position.X - ball.Radius < 0.0f) ball.Position = new PointF(ball.Radius, ball.Position.Y);
            if (ball.Position.X + ball.Radius > size.X) ball.Position = new PointF(size.X - ball.Radius, ball.Position.Y);
            if (ball.Position.Y - ball.Radius < 0.0f) ball.Position = new PointF(ball.Position.X, ball.Radius);
            if (ball.Position.Y + ball.Radius > size.Y) ball.Position = new PointF(ball.Position.X, size.Y - ball.Radius);

            if (enableTarget)
                ball.Position = target;

            if (enableTarget)
                enableTarget = false;

            for (int i = 0; i < rects.Count(); i++)
            {
                RectObject rect = rects[i];
                if (rect.Hit(ball.Position, ball.Radius) == 0) continue;
                (PointF pos, PointF v) =
                    rect.GetReflection(
                        ball.Position,
                        new PointF(
                            ball.Position.X - ball.Velocity.X,
                            ball.Position.Y - ball.Velocity.Y
                        ),
                        ball.Radius
                    );
                ball.Position = pos;
                ball.Velocity = v;
                ball.Velocity = new PointF(
                    ball.Velocity.X * 0.9f,
                    ball.Velocity.Y * 0.9f
                );
            }
        }
        public void Render(Graphics g)
        {
            g.DrawString(
                "スペースキーで当たり判定表示/非表示\nGキーで重力考慮/無視\nRキーで矩形回転/停止\nスクロールでボール半径変更\nクリックでボール位置移動",
                new Font("Ubuntu", 18),
                new SolidBrush(Color.FromArgb(128, 30, 30, 30)),
                new PointF(20.0f, 20.0f)
            );

            foreach (RectObject rect in rects)
            {
                rect.Render(g);
                if (view) rect.RenderHitArea(g, ball.Position, ball.Radius);
            }
            ball.Render(g);

            PointF pos = mouse;
            PointF v = new PointF(
                ball.Position.X - mouse.X,
                ball.Position.Y - mouse.Y
            );

            if (!gr)
            {
                foreach (RectObject rect in rects)
                    if (rect.Hit(mouse, ball.Radius) != 0)
                        (pos, v) = rect.GetReflection(mouse, ball.Position, ball.Radius);
                Pen p = new Pen(Color.FromArgb(255, 255, 0, 0), 3.0f);
                g.DrawEllipse(p, pos.X - ball.Radius, pos.Y - ball.Radius, ball.Radius * 2.0f, ball.Radius * 2.0f);
                g.DrawLine(p, ball.Position, pos);
                g.DrawLine(p, pos, new PointF(pos.X + v.X, pos.Y + v.Y));
            }
            else
            {
                EllipseObject ball_ = new EllipseObject(ball.Position, ball.Radius, 0.8f);
                ball_.Velocity = new PointF(v.X * -0.1f, v.Y * -0.1f);
                ball_.Friction = 0.9f;
                PointF pos_ = ball_.Position;
                int len = 100;
                for (int i = 0; i < len; i++)
                {
                    ball_.Update();
                    ball_.Acceleration = new PointF(0.0f, 0.7f);
                    ball_.Friction = 0.99f;
                    Pen p = new Pen(Color.FromArgb(255 - 255 * i / len, 255, 0, 0), 3.0f);
                    foreach (RectObject rect in rects)
                        if (rect.Hit(ball_.Position, ball_.Radius) != 0)
                        {
                            (ball_.Position, ball_.Velocity) = rect.GetReflection(ball_.Position, pos_, ball_.Radius);
                            ball.Velocity = new PointF(ball.Velocity.X * 0.9f, ball.Velocity.Y * 0.9f);
                            g.DrawEllipse(
                                p, ball_.Position.X - ball_.Radius, ball_.Position.Y - ball_.Radius, ball_.Radius * 2.0f, ball_.Radius * 2.0f
                            );
                        }
                    if (ball_.Position.X - ball_.Radius < 0.0f) ball_.Position = new PointF(ball_.Radius, ball_.Position.Y);
                    if (ball_.Position.X + ball_.Radius > size.X) ball_.Position = new PointF(size.X - ball_.Radius, ball_.Position.Y);
                    if (ball_.Position.Y - ball_.Radius < 0.0f) ball_.Position = new PointF(ball_.Position.X, ball_.Radius);
                    if (ball_.Position.Y + ball_.Radius > size.Y) ball_.Position = new PointF(ball_.Position.X, size.Y - ball_.Radius);
                    g.DrawLine(p, pos_, ball_.Position);
                    pos_ = ball_.Position;
                }
            }
        }
    }
    public class AnimationFloat
    {
        public float
            position,
            velocity,
            acceleration,
            frictionCoefficient;
        public AnimationFloat(
            float position = 0.0f,
            float frictionCoefficient = 0.8f
        )
        {
            this.position = position;
            this.velocity = 0.0f;
            this.acceleration = 0.0f;
            this.frictionCoefficient = frictionCoefficient;
        }
        public void Update()
        {
            velocity *= frictionCoefficient;
            velocity += acceleration;
            position += velocity;
        }
    }
    public class EllipseObject
    {
        private AnimationFloat positionX, positionY, radius;
        public PointF Position
        {
            set
            {
                positionX.position = value.X;
                positionY.position = value.Y;
            }
            get { return new PointF(positionX.position, positionY.position); }
        }
        public PointF Velocity
        {
            set
            {
                positionX.velocity = value.X;
                positionY.velocity = value.Y;
            }
            get { return new PointF(positionX.velocity, positionY.velocity); }
        }
        public PointF Acceleration
        {
            set
            {
                positionX.acceleration = value.X;
                positionY.acceleration = value.Y;
            }
            get { return new PointF(positionX.acceleration, positionY.acceleration); }
        }
        public float Friction
        {
            set
            {
                positionX.frictionCoefficient = value;
                positionY.frictionCoefficient = value;
            }
            get { return positionX.frictionCoefficient; }
        }
        public float Radius
        {
            set { radius.position = value; }
            get { return radius.position; }
        }
        public EllipseObject(
            PointF position,
            float radius,
            float frictionCoefficient = 0.8f
        )
        {
            positionX = new AnimationFloat(position.X, frictionCoefficient);
            positionY = new AnimationFloat(position.Y, frictionCoefficient);
            this.radius = new AnimationFloat(radius, frictionCoefficient);
        }
        public virtual void Update()
        {
            positionX.Update();
            positionY.Update();
            radius.Update();
        }
        public virtual void Render(Graphics g)
        {
            g.ResetTransform();
            g.TranslateTransform(Position.X, Position.Y);
            g.FillEllipse(
                new SolidBrush(Color.FromArgb(255, 30, 30, 30)),
                new RectangleF(
                    -Radius,
                    -Radius,
                    Radius * 2.0f,
                    Radius * 2.0f
                )
            );
            g.ResetTransform();
        }
    }
    public class RectObject
    {
        private AnimationFloat positionX, positionY, sizeX, sizeY, radian;
        public PointF target = new PointF(0.0f, 0.0f);
        public PointF targetSize = new PointF(0.0f, 0.0f);
        public float targetRadian = 0.0f;
        public Color color = Color.FromArgb(255, 30, 30, 30);
        public PointF Position
        {
            set
            {
                positionX.position = value.X;
                positionY.position = value.Y;
            }
            get { return new PointF(positionX.position, positionY.position); }
        }
        public PointF Velocity
        {
            set
            {
                positionX.velocity = value.X;
                positionY.velocity = value.Y;
            }
            get { return new PointF(positionX.velocity, positionY.velocity); }
        }
        public PointF Acceleration
        {
            set
            {
                positionX.acceleration = value.X;
                positionY.acceleration = value.Y;
            }
            get { return new PointF(positionX.acceleration, positionY.acceleration); }
        }
        public PointF Size
        {
            set
            {
                sizeX.position = value.X;
                sizeY.position = value.Y;
            }
            get { return new PointF(sizeX.position, sizeY.position); }
        }
        public float Radian
        {
            set { radian.position = value; }
            get { return radian.position; }
        }
        public RectObject(
            PointF position,
            PointF size,
            float radian = 0.0f,
            float frictionCoefficient = 0.9f
        )
        {
            target = position;
            positionX = new AnimationFloat(position.X, frictionCoefficient);
            positionY = new AnimationFloat(position.Y, frictionCoefficient);
            sizeX = new AnimationFloat(size.X, frictionCoefficient);
            sizeY = new AnimationFloat(size.Y, frictionCoefficient);
            targetSize = size;
            this.radian = new AnimationFloat(radian, frictionCoefficient);
            targetRadian = radian;
        }
        public virtual void Update()
        {
            Acceleration = new PointF((target.X - Position.X) * 0.2f, (target.Y - Position.Y) * 0.2f);
            sizeX.acceleration = (targetSize.X - Size.X) * 0.2f;
            sizeY.acceleration = (targetSize.Y - Size.Y) * 0.2f;
            radian.acceleration = (targetRadian - Radian) * 0.2f;
            positionX.Update();
            positionY.Update();
            sizeX.Update();
            sizeY.Update();
            radian.Update();
        }
        public virtual void Render(Graphics g)
        {
            g.ResetTransform();
            g.TranslateTransform(Position.X, Position.Y);
            g.RotateTransform(-Radian * 360.0f / (float)(Math.PI * 2.0));
            List<PointF> asterisk = new List<PointF>();
            for (int i = 0; i < 15; i++) asterisk.Add(new PointF(
                (float)Math.Cos(Math.PI * 2.0 * (double)i / 15.0) * (i % 3 == 0 ? 4.0f : 9.0f),
                (float)Math.Sin(Math.PI * 2.0 * (double)i / 15.0) * (i % 3 == 0 ? 4.0f : 9.0f)
            ));
            g.FillPolygon(
                new SolidBrush(Color.FromArgb(255, 200, 200, 200)),
                asterisk.ToArray()
            );
            g.FillRectangle(
                new SolidBrush(color),
                new RectangleF(
                    -Size.X / 2.0f,
                    -Size.Y / 2.0f,
                    Size.X,
                    Size.Y
                )
            );
            g.ResetTransform();
        }
        public virtual void RenderHitArea(Graphics g, PointF position, float radius = 0.0f)
        {
            g.ResetTransform();
            g.TranslateTransform(Position.X, Position.Y);
            g.RotateTransform(-Radian * 360.0f / (float)(Math.PI * 2.0));
            List<PointF> asterisk = new List<PointF>();
            for (int i = 0; i < 15; i++) asterisk.Add(new PointF(
                (float)Math.Cos(0.25f * Math.PI * 2.0 * (0.0f + (double)i / 15.0)) * radius + 0.5f * Size.X,
                (float)Math.Sin(0.25f * Math.PI * 2.0 * (0.0f + (double)i / 15.0)) * radius + 0.5f * Size.Y
            ));
            for (int i = 0; i < 15; i++) asterisk.Add(new PointF(
                (float)Math.Cos(0.25f * Math.PI * 2.0 * (1.0f + (double)i / 15.0)) * radius - 0.5f * Size.X,
                (float)Math.Sin(0.25f * Math.PI * 2.0 * (1.0f + (double)i / 15.0)) * radius + 0.5f * Size.Y
            ));
            for (int i = 0; i < 15; i++) asterisk.Add(new PointF(
                (float)Math.Cos(0.25f * Math.PI * 2.0 * (2.0f + (double)i / 15.0)) * radius - 0.5f * Size.X,
                (float)Math.Sin(0.25f * Math.PI * 2.0 * (2.0f + (double)i / 15.0)) * radius - 0.5f * Size.Y
            ));
            for (int i = 0; i < 15; i++) asterisk.Add(new PointF(
                (float)Math.Cos(0.25f * Math.PI * 2.0 * (3.0f + (double)i / 15.0)) * radius + 0.5f * Size.X,
                (float)Math.Sin(0.25f * Math.PI * 2.0 * (3.0f + (double)i / 15.0)) * radius - 0.5f * Size.Y
            ));
            g.DrawPolygon(
                new Pen(Color.FromArgb(255, 128, 128, 128), 3.0f),
                asterisk.ToArray()
            );
            g.ResetTransform();
        }
        public virtual int Hit(PointF position, float radius = 0.0f)
        {
            if (color.A == 0) return 0;
            PointF relativePosition = position;
            relativePosition.X -= Position.X; relativePosition.Y -= Position.Y;
            relativePosition = rotatePointF(relativePosition, Radian);
            // outside
            if (
                (Math.Abs(relativePosition.X) > Size.X / 2.0f + radius) &&
                (Math.Abs(relativePosition.Y) > Size.Y / 2.0f + radius)
            ) return 0;
            // left, bottom, right or right
            else if (
                // left or right
                (
                    (Math.Abs(relativePosition.X) <= Size.X / 2.0f + radius) &&
                    (Math.Abs(relativePosition.Y) <= Size.Y / 2.0f)
                ) ||
                // top or bottom
                (
                    (Math.Abs(relativePosition.X) <= Size.X / 2.0f) &&
                    (Math.Abs(relativePosition.Y) <= Size.Y / 2.0f + radius)
                )
            )
            {
                // get the angle between left top of object and the hitting position
                float radLTHP =
                    (float)Math.Atan2(-relativePosition.Y, relativePosition.X) -
                    (float)Math.Atan2(Size.Y, -Size.X);
                // converting range of angle to (0, 2PI)
                radLTHP = radLTHP - (float)Math.Floor(radLTHP / (float)(Math.PI * 2.0)) * (float)(Math.PI * 2.0);
                // get the angle between left top of object and left bottom of object
                float radLTLB =
                    (float)Math.Atan2(-Size.Y, -Size.X) -
                    (float)Math.Atan2(Size.Y, -Size.X);
                // converting range of angle to (0, 2PI)
                radLTLB = radLTLB - (float)Math.Floor(radLTLB / (float)(Math.PI * 2.0)) * (float)(Math.PI * 2.0);
                radLTLB = (float)Math.Min(radLTLB, (float)(Math.PI * 2.0) - radLTLB);
                // jadge the area include the hitting position
                if (radLTHP < radLTLB) return 1; // left
                else if (radLTHP < (float)Math.PI) return 2; // bottom
                else if (radLTHP < (float)Math.PI + radLTLB) return 3; // right
                else return 4; // right
            }
            // left top
            else if (
                Math.Pow(Size.X / 2.0f + relativePosition.X, 2.0f) +
                Math.Pow(Size.Y / 2.0f + relativePosition.Y, 2.0f) <
                Math.Pow(radius, 2.0)
            ) return 5;
            // left bottom
            else if (
                Math.Pow(Size.X / 2.0f + relativePosition.X, 2.0f) +
                Math.Pow(Size.Y / 2.0f - relativePosition.Y, 2.0f) <
                Math.Pow(radius, 2.0)
            ) return 6;
            // right bottom
            else if (
                Math.Pow(Size.X / 2.0f - relativePosition.X, 2.0f) +
                Math.Pow(Size.Y / 2.0f - relativePosition.Y, 2.0f) <
                Math.Pow(radius, 2.0)
            ) return 7;
            // right top
            else if (
                Math.Pow(Size.X / 2.0f - relativePosition.X, 2.0f) +
                Math.Pow(Size.Y / 2.0f + relativePosition.Y, 2.0f) <
                Math.Pow(radius, 2.0)
            ) return 8;
            else return 0;
        }
        public virtual (PointF pos, PointF vec) GetReflection(PointF currentPosition, PointF prePosition, float radius = 0.0f)
        {
            PointF velocity = new PointF(currentPosition.X - prePosition.X, currentPosition.Y - prePosition.Y);
            if (0 == Hit(currentPosition, radius)) return (currentPosition, velocity);
            (PointF pos, int hit) = (prePosition, Hit(prePosition, radius));
            if (0 == hit) (pos, hit) = GetBorder(currentPosition, prePosition, radius);
            PointF relativePosition = pos;
            relativePosition.X -= Position.X; relativePosition.Y -= Position.Y;
            relativePosition = rotatePointF(relativePosition, Radian);
            float normal = 0.0f;
            velocity.X += Velocity.X;
            velocity.Y -= Velocity.Y;
            switch (hit)
            {
                case 1: // left
                    normal = (float)Math.PI;
                    break;
                case 2: // bottom
                    normal = -(float)Math.PI * 0.5f;
                    break;
                case 3: // right
                    normal = 0.0f;
                    break;
                case 4: // top
                    normal = (float)Math.PI * 0.5f;
                    break;
                case 5: // left top
                    normal = (float)Math.Atan2(-(relativePosition.Y + Size.Y / 2.0f), relativePosition.X + Size.X / 2.0f);
                    break;
                case 6: // left bottom
                    normal = (float)Math.Atan2(-(relativePosition.Y - Size.Y / 2.0f), relativePosition.X + Size.X / 2.0f);
                    break;
                case 7: // right bottom
                    normal = (float)Math.Atan2(-(relativePosition.Y - Size.Y / 2.0f), relativePosition.X - Size.X / 2.0f);
                    break;
                case 8: // right top
                    normal = (float)Math.Atan2(-(relativePosition.Y + Size.Y / 2.0f), relativePosition.X - Size.X / 2.0f);
                    break;
                default: // outside
                    normal = (float)Math.Atan2(-velocity.Y, velocity.X) - Radian;
                    break;
            }
            normal = normal + Radian;
            float vecAngle = 2.0f * ((float)Math.Atan2(-velocity.Y, velocity.X) - (normal + ((float)Math.PI * 0.5f)));
            PointF vec = rotatePointF(velocity, vecAngle);
            if (Math.Abs(vec.X) < 0.01f && Math.Abs(vec.Y) < 0.01f) vec = rotatePointF(new PointF(0.0f, 1.0f), normal);
            return (pos, vec);
        }
        private PointF rotatePointF(PointF position, float radian)
        {
            return new PointF(
                position.X * (float)Math.Cos(radian) - position.Y * (float)Math.Sin(radian),
                position.X * (float)Math.Sin(radian) + position.Y * (float)Math.Cos(radian)
            );
        }
        private (PointF pos, int hit) GetBorder(PointF currentPosition, PointF prePosition, float radius = 0.0f, int nestMax = 8, int nest = 0)
        {
            int hit;
            if (nest >= nestMax) return (currentPosition, Hit(currentPosition, radius));
            PointF middle = new PointF(
                currentPosition.X * 0.5f + prePosition.X * 0.5f,
                currentPosition.Y * 0.5f + prePosition.Y * 0.5f
            );
            if (0 != (hit = Hit(middle, radius)))
                return GetBorder(
                    middle,
                    prePosition,
                    radius,
                    nestMax,
                    nest + 1
                );
            else
                return GetBorder(
                    currentPosition,
                    middle,
                    radius,
                    nestMax,
                    nest + 1
                );
        }
    }
}