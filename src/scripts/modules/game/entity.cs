using System.Numerics;
using Proj.Modules.Graphics;
using SDL2;
using System;

namespace Proj.Modules.Game
{
    public class entity
    {
        public Vector2 position = new Vector2(0, 0);
        
        public IntPtr texture = texture_handler.default_texture;
        public float texture_angle = 0;

        public float texture_xscale = 1;
        public float texture_yscale = 1;

        public int texture_right;
        public int texture_left;
        public int texture_top;
        public int texture_bottom;

        public bool visible = true;

        public virtual void update() {
            uint f;
            int a, w, h;
            SDL.SDL_QueryTexture(texture, out f, out a, out w, out h);
            w = (int)(w * texture_xscale);
            h = (int)(h * texture_yscale);

            texture_right = (int)position.X + w / 2;
            texture_left = (int)position.X - w / 2;
            texture_top = (int)position.Y - h / 2;
            texture_bottom = (int)position.Y + h / 2;
        }

        public virtual void render() {
            uint f;
            int a, w, h;
            SDL.SDL_QueryTexture(texture, out f, out a, out w, out h);
            draw.texture_ext(game_manager.renderer, texture, (int)position.X, (int)position.Y, texture_angle, (int)(w * texture_xscale), (int)(h * texture_yscale), true);
        }
    }
}