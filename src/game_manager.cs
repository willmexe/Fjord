using System;
using static SDL2.SDL;
using static SDL2.SDL_image;
using static SDL2.SDL_ttf;
using Fjord.Modules.Debug;
using Fjord.Modules.Ui;
using Fjord.Modules.Input;
using Fjord.Modules.Misc;
using Fjord.Modules.Graphics;
using Fjord.Modules.Game;
using Fjord.Modules.Mathf;
using Fjord.Game;
using System.IO;
using System.Reflection;
using System.Text;

namespace Fjord
{
    static class game_manager
    {
        public static bool is_running = false;

        public static IntPtr window;
        public static IntPtr renderer;

        public static V2 window_resolution;
        public static V2 resolution;

        public static SDL_Color bg_color;

        private static bool draw_debug = false;

        public static ulong frame_now = SDL_GetPerformanceCounter();
        public static ulong frame_last = 0;

        public static double delta_time_ms = 0;
        public static double delta_time = 0;

        public static string asset_pack = "main";
        public static string executable_path;

        public static string[] sys_args;

        private static int[] fps_avg_arr = new int[120];
        private static int fps_avg_count = 0;

        public static List<string> log = new List<string>();

        public static bool running() {
            return is_running;
        }

        public static void init(string title, int xpos, int ypos, int width, int height, bool fullscreen, string[] sys_args) {

            game_manager.sys_args = sys_args;

            window_resolution = new V2(width, height);
            resolution = new V2(width, height);

            SDL_WindowFlags flags = 0;
            if (fullscreen) {
                flags = SDL_WindowFlags.SDL_WINDOW_FULLSCREEN;
            }

            if (SDL_Init(SDL_INIT_EVERYTHING) == 0) {
                Debug.send("SDL initialized without errors");
                
                window = SDL_CreateWindow(title, xpos, ypos, width, height, flags);

                Debug.send("Window created without errors");

                TTF_Init();

                renderer = SDL_CreateRenderer(window, -1, SDL_RendererFlags.SDL_RENDERER_ACCELERATED);
                SDL_SetRenderDrawBlendMode(renderer, SDL_BlendMode.SDL_BLENDMODE_BLEND);
                
                Debug.send("Renderer created without errors");

                is_running = true;
            } else {
                is_running = false;
            }

            executable_path = Directory.GetCurrentDirectory();

            Language.load_langfile("en_US");

            set_render_background(26, 26, 28, 255);

            zgui.init();
            texture_handler.init();
            font_handler.init();

            scene game_;
            game_ = new game();
            game_.on_load();

            font_handler.load_font("default", "Sans", 42);
        }

        public static void update() {
            fps_avg_arr[fps_avg_count] = (int)(1000 / delta_time_ms);
            fps_avg_count++;

            fps_avg_count = fps_avg_count > fps_avg_arr.Length - 1 ? 0 : fps_avg_count;

            scene_handler.update();
        }

        public static void render() {
            SDL_RenderClear(renderer);

            scene_handler.render();

            if(draw_debug)
                debug_gui.draw_fps();

            SDL_RenderPresent(renderer);

            mouse.llmb = mouse.lmb;
            mouse.lrmb = mouse.rmb;

            for(var i = 0; i < input.pressed_keys.Length; i++) {
                input.last_frame[i] = input.pressed_keys[i];
            }
        }

        public static void stop(Exception e) {

            Debug.error(e.Message + e.StackTrace.Split('\n')[0].Replace(" at ", " In ").Replace("  ", "").Replace("\n", ""));

            log.Add(e.Message + "\n" + e.StackTrace.Replace("   ", ""));

            stop();
        }

        public static void stop() {

            //debug_web.listener.Close();
            scene_handler.stop();

            SDL_DestroyWindow(window);
            SDL_DestroyRenderer(renderer);
            SDL_Quit();

            Debug.send("Game cleaned");

            var time = DateTime.Now.ToString("dd/MMM");
            var file = "logs/" + time + "/" + DateTime.Now.ToString("HH.mm.ss") + ".txt";
            byte[] bytes = Encoding.ASCII.GetBytes("hello");  

            Directory.CreateDirectory("logs/" + time);
            File.WriteAllLines(file, log);

            System.Environment.Exit(0);
        }

        public static int get_fps() {
            return (int)Queryable.Average(fps_avg_arr.AsQueryable());
        }

        public static int get_fps_exact() {
            return (int)(1000 / delta_time_ms);
        }

        [Obsolete("\"tick_fps(int FPS)\" is deprecated. Use \"delta_time\" multiplied to your framerate dependant variables.")]
        public static void tick_fps(int FPS) {
            double frame_delay = 1000 / FPS;

            if (frame_delay > game_manager.delta_time_ms) {
                SDL_Delay((uint)(frame_delay - game_manager.delta_time_ms));
            }
        }

        public static void draw_debug_gui() {
            draw_debug = !draw_debug;
        }

        public static void set_render_resolution(IntPtr renderer, int width, int height) {
            SDL_RenderSetLogicalSize(renderer, width, height);
            resolution.x = width;
            resolution.y = height;
            //SDL_RenderSetLogicalSize(game_manager.renderer, 300, 169);
        }

        public static void set_render_background(byte r, byte g, byte b, byte a) {
            SDL_Color color;
            color.r = r;
            color.g = g;
            color.b = b;
            color.a = a;

            SDL_SetRenderDrawColor(game_manager.renderer, r, g, b, a);

            bg_color = color;
        }

        public static void set_asset_pack(string asset_pack_set) {
            asset_pack = asset_pack_set;
        }

        public static void load_icon() {
            IntPtr icon = IMG_Load("resources/" + game_manager.asset_pack + "/assets/images/icon.png");
            SDL_SetWindowIcon(game_manager.window, icon);
        }
    }
}
