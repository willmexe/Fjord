using System;
using SDL2;
using System.Numerics;
using Proj.Modules.Debug;
using Proj.Modules.Ui;
using Proj.Modules.Input;
using Proj.Modules.Misc;
using Proj.Game;
using System.IO;
using System.Reflection;

namespace Proj
{
    static class game_manager
    {
        public static bool is_running = false;

        public static IntPtr window;
        public static IntPtr renderer;
        
        public static screen_rect screen;

        public static int frame_start = 0;
        public static int frame_length = 0;

        public static string asset_pack = "main";
        public static string executable_path;
        public static string executable_dir;

        public static bool running() {
            return is_running;
        }

        public static void init(string title, int xpos, int ypos, int width, int height, bool fullscreen) {

            SDL.SDL_WindowFlags flags = 0;
            if (fullscreen) {
                flags = SDL.SDL_WindowFlags.SDL_WINDOW_FULLSCREEN;
            }

            if (SDL.SDL_Init(SDL.SDL_INIT_EVERYTHING) == 0) {
                Debug.send("SDL initialized without errors");
                
                window = SDL.SDL_CreateWindow(title, xpos, ypos, width, height, flags);

                Debug.send("Window created without errors");


                renderer = SDL.SDL_CreateRenderer(window, -1, 0);
                SDL.SDL_SetRenderDrawColor(renderer, 47, 49, 90, 255); 
                SDL.SDL_SetRenderDrawBlendMode(renderer, SDL.SDL_BlendMode.SDL_BLENDMODE_BLEND);
                
                Debug.send("Renderer created without errors");

                is_running = true;
            } else {
                is_running = false;
            }

            executable_path = Assembly.GetEntryAssembly().Location;

            string[] executable_arr = game_manager.executable_path.Split("\\");
            Array.Resize(ref executable_arr, executable_arr.Length - 1);
            executable_dir = String.Join("\\", executable_arr);

            Language.load_langfile("en_US");

            screen = new screen_rect();

            scene_handler.add_scene("main", new main_scene());
            scene_handler.load_scene("main");
        }



        public static void update() {
            screen.screen_update();
            scene_handler.update();

            input.last_frame = input.pressed_keys;
            mouse.llmb = mouse.lmb;
            mouse.lrmb = mouse.rmb;
        }

        public static void render() {
            SDL.SDL_RenderClear(renderer);

            scene_handler.render();

            SDL.SDL_RenderPresent(renderer);
        }

        public static void stop() {
            SDL.SDL_DestroyWindow(window);
            SDL.SDL_DestroyRenderer(renderer);
            SDL.SDL_Quit();
            Debug.send("Game cleaned without errors");
        }

        public static void tick_fps(int FPS) {
            frame_length = (int)SDL.SDL_GetTicks() - frame_start;
            int frame_delay = 1000 / FPS;

            if (frame_delay > frame_length) {
                SDL.SDL_Delay((uint)frame_delay - (uint)frame_length);
            }
            frame_start = (int)SDL.SDL_GetTicks();
        }
    }
}
