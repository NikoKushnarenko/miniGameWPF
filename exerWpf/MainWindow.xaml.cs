﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Game;
using System.Windows.Media.Animation;

namespace exerWpf
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private List<ContentControl> stackBox;
        private List<ContentControl> stackEnemy;
        private bool humanCapture;
        private Random random;
        private Robotfactory fact;
        private Robot robot;
        private History GameHistory;
        private Proxy proxy;
        private Earth earth;
        private RobotCommand command;
        private bool gameIsRun = false;
        public MainWindow()
        {
            InitializeComponent();
            HelpWin helpWin = new HelpWin();
            helpWin.ShowDialog();
            helpWin.Close();
            humanCapture = false;
            InitSysRobot();
            GetTemplRobot();
            InitStatus();

        }

        private void InitSysRobot()
        {
            random = new Random();
            stackBox = new List<ContentControl>();
            stackEnemy = new List<ContentControl>();
            fact = new Robotfactory();
            robot = fact.GetRobot(random);
            GameHistory = new History();
            double xHuman = Canvas.GetLeft(human);
            double yHuman = Canvas.GetTop(human);
            GameHistory.Save(robot.State(xHuman, yHuman));
            earth = new Earth();
            command = new RobotCommand(robot);
        }

        private void InitStatus()
        {
            status.Maximum = robot.chargeOfRobot;
            status.Value = robot.chargeOfRobot;
            statusMass.Maximum = robot.MaxMassCargo;
            statusMass.Value = robot.sumOfCargo;
        }
        private void GetTemplRobot()
        {
            string type = robot.GetType().Name;
            if(type == "KiborgRobot")
            {
                myRobot.Template = Resources["r2d2"] as ControlTemplate;
                avatar.Template = Resources["avaR2d2"] as ControlTemplate;
            }
            else if(type == "SaiceRobot")
            {
                myRobot.Template = Resources["s3po"] as ControlTemplate;
                avatar.Template = Resources["avaS3po"] as ControlTemplate;
            }
            else
            {
                myRobot.Template = Resources["bb8"] as ControlTemplate;
                avatar.Template = Resources["avaBb8"] as ControlTemplate;
            }
        }
        private void AddBox()
        {
            ContentControl box = new ContentControl
            {
                Template = Resources["boxForRob"] as ControlTemplate
            };
            Canvas.SetLeft(box, random.Next(100, (int)playArea.ActualWidth - 100));
            Canvas.SetTop(box, random.Next(100, (int) playArea.ActualHeight - 100));
            playArea.Children.Add(box);
            stackBox.Add(box);
            box.MouseEnter += BoxMouseEntered;

        }
        private void AddEnemy()
        {
            ContentControl enemy = new ContentControl
            {
                Template = (random.Next(1, 100) > 50) ? Resources["enemyVayd"] as ControlTemplate : Resources["enemyBen"] as ControlTemplate
            };
            AnimateEnemy(enemy, 0, playArea.ActualWidth - 100, "(Canvas.Left)");
            AnimateEnemy(enemy, random.Next((int)playArea.ActualHeight - 100),
                random.Next((int)playArea.ActualHeight - 100), "(Canvas.Top)");
            playArea.Children.Add(enemy);
            stackEnemy.Add(enemy);
            enemy.MouseEnter += enemy_MouseEnter;

        }

        private void enemy_MouseEnter(object sender, MouseEventArgs e)
        {
            if (humanCapture)
            {
                GameIsOver();
            }
        }

        private void AnimateEnemy(ContentControl enemy, double from, double to, string property)
        {
            Storyboard storyboard = new Storyboard() {AutoReverse = true, RepeatBehavior = RepeatBehavior.Forever};
            DoubleAnimation animation = new DoubleAnimation()
            {
                From = from,
                To = to,
                Duration = new Duration(TimeSpan.FromSeconds(random.Next(4, 6)))
            };
            Storyboard.SetTarget(animation,enemy);
            Storyboard.SetTargetProperty(animation, new PropertyPath(property));
            storyboard.Children.Add(animation);
            storyboard.Begin();
        }

        private void BoxMouseEntered(object sender, MouseEventArgs e)
        {
            if (humanCapture)
            {
                double xHuman = Canvas.GetLeft(human);
                double yHuman = Canvas.GetTop(human);
                int indexEnemy = -1;
                double minDistance = 10000;
                for (int i = 0; i < stackBox.Count; i++)
                {
                    double xEnemy = Canvas.GetLeft(stackBox[i]);
                    double yEnemy = Canvas.GetTop(stackBox[i]);
                    double Distance = Math.Sqrt(Math.Pow((xHuman - xEnemy), 2) + Math.Pow((yHuman - yEnemy), 2));
                    if (Distance < minDistance)
                    {
                        minDistance = Distance;
                        indexEnemy = i;
                    }

                }
                proxy = new Proxy(robot)
                {
                    box = earth.GetBox()
                };
                //MessageBox.Show(String.Format("Price: {0} \rMass: {1} \rХотите подобрать ?",proxy.box.Price, proxy.box.Mass ), "My App", MessageBoxButton.YesNoCancel);
                GameHistory.Save(robot.State(xHuman, yHuman));
                robot.GetGrooz(proxy);
                status.Value = robot.chargeOfRobot;
                statusMass.Value = robot.sumOfCargo;
                GetPosBox(indexEnemy);
            }
        }

        private void GetPosBox(int indexForDel)
        {
            playArea.Children.Remove(stackBox[indexForDel]);
            stackBox.RemoveAt(indexForDel);
        }
        private void PlayArea_MouseMove(object sender, MouseEventArgs e)
        {
            if (humanCapture)
            {
                Point pointerPosition = e.GetPosition(null);
                Point relativePosition = playArea.TransformToVisual(playArea).Transform(pointerPosition);
                if ((Math.Abs(relativePosition.X - Canvas.GetLeft(human)) > human.ActualWidth * 3) ||
                    (Math.Abs(relativePosition.Y - Canvas.GetTop(human)) > human.ActualHeight * 3))
                {
                    humanCapture = false;
                    human.IsHitTestVisible = true;

                }
                else
                {
                    Canvas.SetLeft(human, relativePosition.X - human.ActualWidth / 2);
                    Canvas.SetTop(human, relativePosition.Y - human.ActualHeight / 0.8);
                }
            }
        }
        private void Titul_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            this.DragMove();
        }
        private void Human_MouseDown(object sender, MouseButtonEventArgs e)
        {
            humanCapture = true;
            human.IsHitTestVisible = false;

        }
        private void Test_Click(object sender, RoutedEventArgs e)
        {
            command.Undo(GameHistory);
            status.Value = robot.chargeOfRobot;
        }

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Space)
            {
                AddBox();
            }
            if (e.Key == Key.A)
            {
                humanCapture = false;
                human.IsHitTestVisible = true;
            }
        }

        private void ToggleButton_Checked(object sender, RoutedEventArgs e)
        {
            AddBox();
            AddEnemy();
        }

        private void GameIsOver()
        {
            gameOver.Visibility = Visibility;
            Canvas.SetLeft(human, 10);
            Canvas.SetTop(human, 10);
            ClearStackBoxOrEnemy(stackBox);
            ClearStackBoxOrEnemy(stackEnemy);

        }

        private void ClearStackBoxOrEnemy(List<ContentControl> steckForClear)
        {
            if (steckForClear.Count != 0)
            {
                for (int i = 0; i < steckForClear.Count; i++)
                {
                    playArea.Children.Remove(steckForClear[i]);
                }
                steckForClear.Clear();
            }
            
        }
    }
}
