using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;

using Emgu.CV;                  //
using Emgu.CV.CvEnum;           // usual Emgu CV imports
using Emgu.CV.Structure;        //
using Emgu.CV.UI;
using Emgu.Util;
using Emgu.CV.Util;
//


namespace OpenCV3
{
    public partial class Form1 : Form
    {
        CascadeClassifier haar;
        Form2 info = new Form2();
        bool logic = false;
        int cont1 = 0;
        int cont2 = 0;
        int cont3 = 0;
        int cont4 = 0;
        int cont5 = 0;


        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            haar = new CascadeClassifier("haarcascade_frontalface_alt.xml");
        }


        private void button1_Click(object sender, EventArgs e)
        {
            Bitmap MyImage;
            OpenFileDialog open_dialog = new OpenFileDialog(); //создание диалогового окна для выбора файла
            open_dialog.Filter = "Image Files(*.BMP;*.JPG;*.GIF;*.PNG)|*.BMP;*.JPG;*.GIF;*.PNG|All files (*.*)|*.*"; //формат загружаемого файла
            if (open_dialog.ShowDialog() == DialogResult.OK) //если в окне была нажата кнопка "ОК"
            {

                try
                {
                    MyImage = new Bitmap(open_dialog.FileName);
                    Image<Rgb, Byte> imageOriginal111 = new Image<Rgb, Byte>(MyImage);
                    //вместо pictureBox1 укажите pictureBox, в который нужно загрузить изображение 
                    ibOriginal.Image = imageOriginal111;
                    ibOriginal.Invalidate();
                }
                catch
                {
                    DialogResult rezult = MessageBox.Show("Невозможно открыть выбранный файл",
                    "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            int decision;
            Bitmap orig = ibOriginal.Image.Bitmap;
            if (orig == null)
            {
                MessageBox.Show("no image found" + Environment.NewLine +
                                "exiting program");
                Environment.Exit(0);
                return;
            }
            Image<Bgr, Byte> imageCV = new Image<Bgr, byte>(orig);
            Mat imgOriginal = imageCV.Mat;

            Image<Rgb, Byte> imageOriginal = imgOriginal.ToImage<Rgb, Byte>();
            var rectangles = haar.DetectMultiScale(imageOriginal, 1.1, 3);
            if (rectangles.Length == 0)
            {
            }
            string s = string.Join("\n", rectangles.Select((rect) => rect.X + " " + rect.Y + " " + rect.Width + " " + rect.Height));

            string[] arr = s.Split(' ');

            Mat imgGrayscale = new Mat(imgOriginal.Size, DepthType.Cv8U, 1);
            Mat imgBlurred = new Mat(imgOriginal.Size, DepthType.Cv8U, 1);
            Mat imgCanny = new Mat(imgOriginal.Size, DepthType.Cv8U, 1);

            CvInvoke.CvtColor(imgOriginal, imgGrayscale, ColorConversion.Bgr2Gray);

            CvInvoke.GaussianBlur(imgGrayscale, imgBlurred, new Size(5, 5), 1.5);

            CvInvoke.Canny(imgBlurred, imgCanny, 180, 100);

            var contours = new VectorOfVectorOfPoint();
            var contours_Of_man = new VectorOfVectorOfPoint();

            CvInvoke.FindContours(imgCanny, contours, null, RetrType.Tree, ChainApproxMethod.ChainApproxSimple);

            try
            {
                //получаем данные о прямоугольнике - координаты левого нижнего угла и размеры
                int X = Convert.ToInt32(arr[0]);
                int Y = Convert.ToInt32(arr[1]);
                int W = Convert.ToInt32(arr[2]);
                int H = Convert.ToInt32(arr[3]);
                //вычисляем координаты всех четырех углов
                int x1, y1, x2, y2, x3, y3, x4, y4;
                x1 = X;
                y1 = Y;
                x2 = X;
                y2 = Y + H;
                x3 = X + W;
                y3 = Y + H;
                x4 = X + W;
                y4 = Y;

                double left, right, up, down, center_1_x, center_1_y, center_2_x, center_2_y;

                //находим координаты центра лица и центра изображения
                center_1_x = Convert.ToDouble(X) + (Convert.ToDouble(W)) / 2.0;
                center_1_y = Convert.ToDouble(Y) + (Convert.ToDouble(H)) / 2.0;
                center_2_x = (Convert.ToDouble(ibOriginal.Width)) / 2.0;
                center_2_y = (Convert.ToDouble(ibOriginal.Height)) / 2.0;

                //вычисляем на сколько в градусах надо повернуть камеру чтобы центры лица и изображения совпали
                if (center_1_x > center_2_x)
                {
                    left = 0.0;
                    right = (center_1_x - center_2_x) / ((Convert.ToDouble(ibOriginal.Width)) / 50.0);
                }
                else
                {
                    right = 0.0;
                    left = (center_2_x - center_1_x) / ((Convert.ToDouble(ibOriginal.Width)) / 50.0);
                }
                if (center_1_y > center_2_y)
                {
                    down = 0.0;
                    up = (center_1_y - center_2_y) / ((Convert.ToDouble(ibOriginal.Height)) / 45.0);
                }
                else
                {
                    up = 0.0;
                    down = (center_2_y - center_1_y) / ((Convert.ToDouble(ibOriginal.Height)) / 45.0);
                }

                //передаем данные на вторую форму 
                CallBackMy.callbackEventHandler(left, right, down, up);

                //выделяем лицо прямоугольником
                Bitmap image1;

                image1 = new Bitmap(imgOriginal.Bitmap);
                Graphics g1 = Graphics.FromImage(image1);
                g1.DrawLine(new Pen(Brushes.Red, 2), x1, y1, x2, y2);
                g1.DrawLine(new Pen(Brushes.Red, 2), x2, y2, x3, y3);
                g1.DrawLine(new Pen(Brushes.Red, 2), x3, y3, x4, y4);
                g1.DrawLine(new Pen(Brushes.Red, 2), x4, y4, x1, y1);

                Image<Rgb, Byte> imageOriginal1 = new Image<Rgb, Byte>(image1);

                //выбираем контуры, которые принадлежат человеку.

                int x_cont_man_1, x_cont_man_2, y_cont_man_1, y_cont_man_2;

                x_cont_man_1 = X - W;
                x_cont_man_2 = X + 2 * W;
                y_cont_man_1 = Y - H / 2;
                y_cont_man_2 = Y + 6 * H;

                contours_Of_man.Clear();

                for (int i = 0; i < contours.Size; i++)
                {
                    bool logic1 = true;
                    for (int j = 0; j < contours[i].Size; j++)
                    {
                        if (contours[i][j].X < x_cont_man_1 || contours[i][j].X > x_cont_man_2 || contours[i][j].Y < y_cont_man_1 || contours[i][j].Y > y_cont_man_2)
                        {
                            logic1 = false;
                            j = contours[i].Size;
                        }
                    }
                    if (logic1)
                    {
                        VectorOfPoint mat = new VectorOfPoint();
                        mat = contours[i];
                        contours_Of_man.Push(mat);
                    }
                }
                Mat imagCanny = new Mat(imgOriginal.Size, DepthType.Cv8U, 1);
                imagCanny.SetTo(new MCvScalar(0, 0, 0));
                for (int h = 0; h < contours_Of_man.Size; h++)
                {
                    CvInvoke.DrawContours(imagCanny, contours_Of_man, h, new MCvScalar(255, 0, 255, 0));
                }

                ibOriginal.Image = imageOriginal1;
                ibCanny.Image = imagCanny;
                logic = true;
                double le = 0.0;
                double ri = 0.0;
                double u = 0.0;
                double dow = 0.0;
                try
                {
                   le = Convert.ToDouble(textBox2.Text);
                   ri = Convert.ToDouble(textBox3.Text);
                   u = Convert.ToDouble(textBox4.Text);
                   dow = Convert.ToDouble(textBox5.Text);
                }
                catch (Exception ea)
                {
                    MessageBox.Show(ea.Message + Environment.NewLine +
                "exiting program");
                    Environment.Exit(0);
                    return;
                }
                decision = MakeDecision(true, W, H, X, Y, orig.Width, orig.Height, u, dow, le, ri);
            }
            catch (Exception ea)
            {
                MessageBox.Show("Face no detected");
                CallBackMy.callbackEventHandler(100.0, 100.0, 100.0, 100.0);
                Image<Rgb, Byte> imageOriginal111 = new Image<Rgb, Byte>(orig);
                //вместо pictureBox1 укажите pictureBox, в который нужно загрузить изображение 
                ibOriginal.Image = imageOriginal111;
                ibOriginal.Invalidate();
                if (logic)
                {
                    Mat imagCanny = new Mat(imgOriginal.Size, DepthType.Cv8U, 1);
                    imagCanny.SetTo(new MCvScalar(0, 0, 0));
                    for (int h = 0; h < contours.Size; h++)
                    {
                        CvInvoke.DrawContours(imagCanny, contours, h, new MCvScalar(255, 0, 255, 0));
                    }
                    ibCanny.Image = imagCanny;
                }
                else
                {
                    ibCanny.Image = imgCanny;
                }
                decision = MakeDecision(false, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0);
            }
            textBox1.Text = textDecision(decision);

        }

        int MakeDecision(bool Face_In_Frame, int X_1, int Y_1, int X_left, int Y_down, int Size_X, int Size_Y, double Front, double Back, double Left, double Right)
        {
            int X = X_1;//ширина лица человека
            int Y = Y_1;//высота лица человека
            int X_x = 200;//желаемая ширина лица человека
            int Y_x = 400;//желаемая высота лица человека
            int dX = 10;//допустимое измнение ширины лица
            int dY = 10;//допустимое изменение высоты лица
            int X_l = X_left;//расстояние от левого края лица до левого края кадра
            int X_r = Size_X - X_l - X;//расстояние от правого края лица до правого края кадра
            int Y_d = Y_down;//расстояние от нижнего края лица до нижнего края кадра
            int Y_u = Size_Y - Y_1 - Y_d;//расстояние от верхнего края лица до верхнего края кадра
            int dX_f = 10;// допустимое отклонение лица от центра диаграммы
            int dY_u = 10;// Допустимое отклонение лица от центра диаграммы
            bool face_in_frame = Face_In_Frame;// лицо в кадре
            bool turn_camera_left = false;//повернуть камеру влево
            bool turn_camera_right = false;//повернуть камеру вправо
            bool turn_camera_up = false;// поднять камеру наверх
            bool turn_camera_down = false;//опустить камеру вниз
            bool move_left = false;//движение налево
            bool move_right = false;//движени направо
            bool move_forward = false;//движение вперед
            bool move_back = false;//движение назад
            double FrontMin = 0.3;//минимально допустимое растояние до препятствия спереди
            double LeftMin = 0.3;//минимально допустимое растояние до препятствия слева
            double RightMin = 0.3;//минимально допустимое растояние до препятствия справа 
            double BackMin = 0.3;//минимально допустимое растояние до препятствия зади
            double SensorFront = Front;//данные с датчика
            double SensorLeft = Left;//данные с датчика
            double SensorRight = Right;//данные с датчика
            double SensorBack = Back;//данные с датчика
            int[] buffer = { 0, 0, 0, 0, 0 };//буффер действия робота
            double[] buffer_X = { 0, 0, 0, 0, 0 }; //буффер ширины лица за последние циклы
            bool left, right, forward, back; //данные о движении робота в последние моменты времени
                                             //0 - покой
                                             //1 - движение налево
                                             //2 - движение навправо
                                             //3 - движение вперед
                                             //4 - движение назад
            left = right = forward = back = false;
            for (int i = 0; i < 5; i++)
            {
                if (buffer[i] == 1)
                {
                    left = true;
                    right = false;
                }
                if (buffer[i] == 2)
                {
                    left = false;
                    right = true;
                }
                if (buffer[i] == 3)
                {
                    forward = true;
                    back = false;
                }
                if (buffer[i] == 4)
                {
                    back = true;
                    forward = false;
                }
            }

            if (face_in_frame == true)
            {
                if (((X_r - dX_f) < X_l) && (X_l < (X_r + dX_f)))
                {
                    if (((Y_d - dY_u) < Y_u) && (Y_u < (Y_d + dY_u)))
                    {
                        if ((X > (X_x - dX)) && ((Y_x - dY) < Y) && (Y < (Y_x + dY)))
                        {
                            turn_camera_down = false;
                            turn_camera_left = false;
                            turn_camera_right = false;
                            turn_camera_up = false;
                            move_back = false;
                            move_forward = false;
                            move_left = false;
                            move_right = false;
                        }
                        else
                        {
                            if (Y < (Y_x - dY))
                            {
                                if (SensorFront > FrontMin)
                                {
                                    move_forward = true;
                                }
                                else
                                {
                                    if (left == true)
                                    {
                                        if (SensorLeft > LeftMin)
                                        {
                                            move_left = true;
                                        }
                                        else
                                        {
                                            SensorRight_Min();
                                        }
                                    }
                                    else
                                    {
                                        if (right == true)
                                        {
                                            if (SensorRight > RightMin)
                                            {
                                                move_right = true;
                                            }
                                            else
                                            {
                                                SensorLeft_Min();
                                            }
                                        }
                                        else
                                        {
                                            if (SensorRight > SensorLeft)
                                            {
                                                SensorRight_Min();
                                            }
                                            //Этого изначальное не было, но по UML диаграмме должен быть
                                            else
                                            {
                                                SensorLeft_Min();
                                            }
                                        }
                                    }
                                }
                            }
                            else
                            {
                                if (Y > (Y_x + dY))
                                {
                                    if (SensorBack > BackMin)
                                    {
                                        move_back = true;
                                    }
                                    else
                                    {
                                        if (left == true)
                                        {
                                            if (SensorLeft > LeftMin)
                                            {
                                                move_left = true;
                                            }
                                            else
                                            {
                                                SensorRight_Min();
                                            }
                                        }
                                        if (right == true)
                                        {
                                            if (SensorRight > RightMin)
                                            {
                                                move_right = true;
                                            }
                                            else
                                            {
                                                SensorLeft_Min();
                                            }
                                        }
                                        else
                                        {
                                            if (SensorRight > SensorLeft)
                                            {
                                                SensorRight_Min();
                                            }
                                            else
                                            {
                                                SensorLeft_Min();
                                            }
                                        }
                                    }
                                }
                                else
                                {
                                    //Код
                                    if (left == true)
                                    {
                                        if (X - buffer_X[0] > 0)
                                        {
                                            SensorLeft_Min();
                                        }
                                        else
                                        {
                                            SensorRight_Min();
                                        }
                                    }
                                    else
                                    {
                                        if (right == true)
                                        {
                                            if (X - buffer_X[0] > 0)
                                            {
                                                SensorRight_Min();
                                            }
                                            else
                                            {
                                                SensorLeft_Min();
                                            }
                                        }
                                        else
                                        {
                                            if (SensorLeft > SensorRight)
                                            {
                                                SensorLeft_Min();
                                            }
                                            else
                                            {
                                                SensorRight_Min();
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                    else
                    {
                        if (Y_u > Y_d)
                        {
                            turn_camera_down = true;
                        }
                        else turn_camera_up = true;
                    }
                }
                else
                {
                    if (X_l > X_r)
                    {
                        turn_camera_left = true;
                    }
                    else turn_camera_right = true;
                }

            }
            else turn_camera_left = true;

            buffer[0] = buffer[1];
            buffer[1] = buffer[2];
            buffer[2] = buffer[3];
            buffer[3] = buffer[4];

            if (move_left)
            {
                buffer[4] = 1;
            }
            else if (move_right)
            {
                buffer[4] = 2;
            }
            else if (move_forward)
            {
                buffer[4] = 3;
            }
            else if (move_back)
            {
                buffer[4] = 4;
            }
            else
            {
                buffer[4] = 0;
            }


            //Для проверки увеличени ширины лица
            buffer_X[0] = buffer_X[1];
            buffer_X[1] = buffer_X[2];
            buffer_X[2] = buffer_X[3];
            buffer_X[3] = buffer_X[4];
            buffer_X[4] = X;
            if (turn_camera_left)
            {
                return 1;
            }
            if (turn_camera_right)
            {
                return 2;
            }
            if (turn_camera_up)
            {
                return 3;
            }
            if (turn_camera_down)
            {
                return 4;
            }
            if (move_left)
            {
                return 5;
            }
            if (move_right)
            {
                return 6;
            }
            if (move_back)
            {
                return 7;
            }
            if (move_forward)
            {
                return 8;
            }

            return 0;

            //Методы поворота, название совпадает с условием
            void SensorLeft_Min()
            {
                if (SensorLeft > LeftMin)
                {
                    move_left = true;
                }
                else
                {
                    turn_camera_down = false;
                    turn_camera_left = false;
                    turn_camera_right = false;
                    turn_camera_up = false;
                    move_back = false;
                    move_forward = false;
                    move_left = false;
                    move_right = false;
                }
            }

            void SensorRight_Min()
            {
                if (SensorRight > RightMin)
                {
                    move_right = true;
                }
                else
                {
                    turn_camera_down = false;
                    turn_camera_left = false;
                    turn_camera_right = false;
                    turn_camera_up = false;
                    move_back = false;
                    move_forward = false;
                    move_left = false;
                    move_right = false;
                }
            }
        }

        string textDecision(int decision)
        {
            if (decision == 0)
            {
                return "Immobility";
            }
            if (decision == 1)
            {
                return "Turn camera right";
            }
            if (decision == 2)
            {
                return "Turn camera left";
            }
            if (decision == 3)
            {
                return "Turn camera up";
            }
            if (decision == 4)
            {
                return "Turn camera down";
            }
            if (decision == 5)
            {
                return "Move left";
            }
            if (decision == 6)
            {
                return "Move right";
            }
            if (decision == 7)
            {
                return "Move back";
            }
            if (decision == 8)
            {
                return "Move forward";
            }
            return "";
        }
    }
}