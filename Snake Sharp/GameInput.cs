using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace Snake_Sharp
{
    public class GameInput
    {
        #region DLL IMPORT
        // DLLs checking if the application has focus
        [DllImport("user32.dll", CharSet = CharSet.Auto, ExactSpelling = true)]
        private static extern IntPtr GetForegroundWindow();
        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern int GetWindowThreadProcessId(IntPtr handle, out int processId);

        // DLLs for global keyboard hooking
        [DllImport("user32.dll")]
        private static extern IntPtr SetWindowsHookEx(int idHook, keyboardHookProc callback, IntPtr hInstance, uint threadId);
        [DllImport("user32.dll")]
        private static extern bool UnhookWindowsHookEx(IntPtr hInstance);
        [DllImport("user32.dll")]
        private static extern int CallNextHookEx(IntPtr idHook, int nCode, int wParam, ref KeyboardHookStruct lParam);
        [DllImport("kernel32.dll")]
        private static extern IntPtr LoadLibrary(string lpFileName);
        #endregion

        public delegate int keyboardHookProc(int code, int wParam, ref KeyboardHookStruct lParam);

        public struct KeyboardHookStruct
        {
            public int vkCode;
        }

        // Declaration of engine parts
        private Main mainClass;
        private GraphicsEngine graphicsEngineClass;

        // Windows messages
        private const int WH_KEYBOARD_LL = 13;
        private const int WM_KEYDOWN = 0x100;
        private const int WM_KEYUP = 0x101;
        private const int WM_SYSKEYDOWN = 0x104;
        private const int WM_SYSKEYUP = 0x105;

        //private List<Keys> HookedKeys = new List<Keys>();

        private IntPtr hhook = IntPtr.Zero;

        private keyboardHookProc SAFE_delegate_callback;

        private bool directionSet = false;

        public enum SnakeDirection
        {
            Left,
            Right,
            Up,
            Down
        }

        public SnakeDirection CurrentDirectionPublic { get; set; } = SnakeDirection.Right;

        public void SetHook()
        {
            IntPtr hInstance = LoadLibrary("User32");
            SAFE_delegate_callback = new keyboardHookProc(HookProc);
            hhook = SetWindowsHookEx(WH_KEYBOARD_LL, SAFE_delegate_callback, hInstance, 0);

            Console.WriteLine(DateTime.Now.ToString("[yyyy-MM-dd - hh:mm:ss] ") + "Game Input initialized");
        }

        public void UnHook()
        {
            UnhookWindowsHookEx(hhook);
        }

        public void SetEngineReference(Main _PassedMainClass, GraphicsEngine _PassedGraphicsEngine)
        {
            mainClass = _PassedMainClass;
            graphicsEngineClass = _PassedGraphicsEngine;
        }

        // Reenable direction set
        public void ResetKeystate()
        {
            if (directionSet) { directionSet = false; }
        }

        // Hook loop
        private int HookProc(int code, int wParam, ref KeyboardHookStruct lParam)
        {
            try
            {
                if (code >= 0 && IsApplicationActivated())
                {
                    Keys key = (Keys)lParam.vkCode;

                    KeyEventArgs kea = new KeyEventArgs(key);
                    Debug.Print(wParam.ToString());
                    if (wParam == 256)         // If a key is down     
                    {
                        switch (key)
                        {
                            case Keys.Left:
                            case Keys.A:
                                Console.WriteLine(DateTime.Now.ToString("[yyyy-MM-dd - hh:mm:ss] ") + "Direction: left");
                                if (!directionSet)
                                {
                                    if (graphicsEngineClass.SnakeHolderPublic.Count == 1)
                                    {
                                        CurrentDirectionPublic = SnakeDirection.Left;

                                    }
                                    else if (graphicsEngineClass.SnakeHolderPublic.Count > 1 && CurrentDirectionPublic != SnakeDirection.Right)
                                    {
                                        CurrentDirectionPublic = SnakeDirection.Left;
                                    }
                                    directionSet = true;
                                }
                                break;
                            case Keys.Right:
                            case Keys.D:
                                Console.WriteLine(DateTime.Now.ToString("[yyyy-MM-dd - hh:mm:ss] ") + "Direction: right");
                                if (!directionSet)
                                {
                                    if (graphicsEngineClass.SnakeHolderPublic.Count == 1)
                                    {
                                        CurrentDirectionPublic = SnakeDirection.Right;
                                    }
                                    else if (graphicsEngineClass.SnakeHolderPublic.Count > 1 && CurrentDirectionPublic != SnakeDirection.Left)
                                    {
                                        CurrentDirectionPublic = SnakeDirection.Right;
                                    }
                                    directionSet = true;
                                }
                                break;
                            case Keys.Up:
                            case Keys.W:
                                Console.WriteLine(DateTime.Now.ToString("[yyyy-MM-dd - hh:mm:ss] ") + "Direction: up");
                                if (!directionSet)
                                {
                                    if (graphicsEngineClass.SnakeHolderPublic.Count == 1)
                                    {
                                        CurrentDirectionPublic = SnakeDirection.Up;
                                    }
                                    else if (graphicsEngineClass.SnakeHolderPublic.Count > 1 && CurrentDirectionPublic != SnakeDirection.Down)
                                    {
                                        CurrentDirectionPublic = SnakeDirection.Up;
                                    }
                                    directionSet = true;
                                }
                                break;
                            case Keys.Down:
                            case Keys.S:
                                Console.WriteLine(DateTime.Now.ToString("[yyyy-MM-dd - hh:mm:ss] ") + "Direction: down");
                                if (!directionSet)
                                {
                                    if (graphicsEngineClass.SnakeHolderPublic.Count == 1)
                                    {
                                        CurrentDirectionPublic = SnakeDirection.Down;
                                    }
                                    else if (graphicsEngineClass.SnakeHolderPublic.Count > 1 && CurrentDirectionPublic != SnakeDirection.Up)
                                    {
                                        CurrentDirectionPublic = SnakeDirection.Down;
                                    }
                                    directionSet = true;
                                }
                                break;
                            case Keys.Q:
                                Console.WriteLine(DateTime.Now.ToString("[yyyy-MM-dd - hh:mm:ss] ") + "Restarting game");
                                graphicsEngineClass.DefaultGameState();
                                
                                break;
                            case Keys.J:
                                if (mainClass.MainGameLoop.Enabled) { mainClass.MainGameLoop.Stop(); }
                                break;
                            case Keys.Escape:

                                //if (!GameLoopTimer.Enabled && !GameOverMenu.Enabled)
                                //{
                                //    Settings.Left = (this.ClientSize.Width - Settings.Width) / 2;
                                //    Settings.Top = (this.ClientSize.Height - Settings.Height) / 2;

                                //    GameSpeedSettings.Focus();

                                //    if (Settings.Enabled && Settings.Visible)
                                //    {
                                //        Settings.Enabled = false;
                                //        Settings.Visible = false;
                                //    }
                                //    else
                                //    {
                                //        Settings.Enabled = true;
                                //        Settings.Visible = true;
                                //    }
                                //}
                                //break;
                            //case Keys.J:
                            //    if (GameOverMenu.Enabled && GameOverMenu.Visible)
                            //    {
                            //        RestartGame();

                            //        GameOverMenu.Enabled = false;
                            //        GameOverMenu.Visible = false;
                            //    }
                            //    break;
                            //case Keys.N:
                            //    if (GameOverMenu.Enabled && GameOverMenu.Visible)
                            //    {
                            //        GameOverMenu.Enabled = false;
                            //        GameOverMenu.Visible = false;
                            //    }
                            //    break;
                            default:
                                break;
                        }
                        //if ((wParam == WM_KEYDOWN || wParam == WM_SYSKEYDOWN) && (KeyDown != null))
                        //{
                        //    
                        //    KeyDown(this, kea);
                        //}
                        //else if ((wParam == WM_KEYUP || wParam == WM_SYSKEYUP) && (KeyUp != null))
                        //{
                        //    KeyUp(this, kea);
                        //}
                        if (kea.Handled)
                            return 1;

                    }
                    else if (wParam == 257)         // If a key is released
                    {
                        switch (key)
                        {
                            case Keys.Q:
                                //if (!Main_Class.gameRunningPublic) { Main_Class.gameRunningPublic = true; }
                                break;
                            default:
                                break;
                        }


                    }
                }


                return CallNextHookEx(hhook, code, wParam, ref lParam);
            }
            catch (Exception err)
            {
                // TODO: Log it
                return 0;
            }
        }

        public bool IsApplicationActivated()
        {
            IntPtr activatedHandle = GetForegroundWindow();
            if (activatedHandle == IntPtr.Zero)
            {
                return false;       // No window is currently activated
            }

            int applicationProccessId = Process.GetCurrentProcess().Id;

            GetWindowThreadProcessId(activatedHandle, out int activeProccessId);

            return activeProccessId == applicationProccessId;
        }

        //protected override void WndProc(ref Message msg)
        //{
        //    if (msg.Msg == 0x319)   // WM_APPCOMMAND message
        //    {
        //        // extract cmd from LPARAM (as GET_APPCOMMAND_LPARAM macro does)
        //        int cmd = (int)((uint)msg.LParam >> 16 & ~0xf000);
        //        switch (cmd)
        //        {
        //            case 13:  // APPCOMMAND_MEDIA_STOP constant
        //                MessageBox.Show("Stop");
        //                break;
        //            case 14:  // APPCOMMAND_MEDIA_PLAY_PAUSE
        //                MessageBox.Show("Play/Pause");
        //                break;
        //            case 11:  // APPCOMMAND_MEDIA_NEXTTRACK
        //                MessageBox.Show("Next");
        //                break;
        //            case 12:  // APPCOMMAND_MEDIA_PREVIOUSTRACK
        //                MessageBox.Show("Previous");
        //                break;
        //            default:
        //                break;
        //        }
        //    }
        //    base.WndProc(ref msg);
        //}

        
    }


    //public class IDHook
    //{
    //    public const int WH_CALLWNDPROC = 4;
    //    public const int WH_CALLWNDPROCRET = 12;
    //    public const int WH_CBT = 5;
    //    public const int WH_DEBUG = 9;
    //    public const int WH_FOREGROUNDIDLE = 11;
    //    public const int WH_GETMESSAGE = 3;
    //    public const int WH_JOURNALPLAYBACK = 1;
    //    public const int WH_JOURNALRECORD = 0;
    //    public const int WH_KEYBOARD = 2;
    //    public const int WH_KEYBOARD_LL = 13;
    //    public const int WH_MOUSE = 7;
    //    public const int WH_MOUSE_LL = 14;
    //    public const int WH_MSGFILTER = -1;
    //    public const int WH_SHELL = 10;
    //    public const int WH_SYSMSGFILTER = 6;
    //}
}

























































//// DLLs for global keyboard hooking
//[DllImport("user32.dll")]
//private static extern IntPtr SetWindowsHookEx(int idHook, LowLevelKeyboardProc callback, IntPtr hInstance, uint threadId);
//[DllImport("user32.dll")]
//private static extern bool UnhookWindowsHookEx(IntPtr hInstance);
//[DllImport("user32.dll")]
//private static extern IntPtr CallNextHookEx(IntPtr idHook, int nCode, int wParam, IntPtr lParam);
//[DllImport("kernel32.dll")]
//private static extern IntPtr LoadLibrary(string lpFileName);

//private delegate IntPtr LowLevelKeyboardProc(int nCode, IntPtr wParam, IntPtr lParam);
//private LowLevelKeyboardProc _proc = hookProc;
//private static IntPtr hhook = IntPtr.Zero;
//private static bool ctrlPressed = false;


//public void SetHook()
//{
//    IntPtr hInstance = LoadLibrary("User32");
//    hhook = SetWindowsHookEx(idHook.WH_KEYBOARD_LL, _proc, hInstance, 0);
//}


//public void UnHook()
//{
//    UnhookWindowsHookEx(hhook);
//}


//private static IntPtr hookProc(int code, IntPtr wParam, IntPtr lParam)
//{
//    try
//    {
//        if (code >= 0 && wParam == (IntPtr)0x100)
//        {
//            //if (ApplicationIsActivated())
//            //{

//            int vkCode = Marshal.ReadInt32(lParam);
//            Keys key = (Keys)vkCode;
//            KeyEventArgs kea = new KeyEventArgs(key);

//            if (vkCode == 162 || vkCode == 163)
//            {
//                ctrlPressed = true;
//            }
//            else if (vkCode == 88 && ctrlPressed == true)
//            {
//                MessageBox.Show(vkCode.ToString());
//            }
//            else
//            {
//                ctrlPressed = false;
//            }
//            //}
//            if (!kea.Handled)
//                return (IntPtr)1;
//        }

//        return CallNextHookEx(hhook, code, (int)wParam, lParam);

//    }
//    catch
//    {

//        return (IntPtr)0;
//    }

//}