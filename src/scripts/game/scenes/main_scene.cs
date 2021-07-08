using Proj.Modules.Input;
using Proj.Modules.Ui;
using SDL2;

namespace Proj.Game {
    public class main_scene : scene {
        int x, y = 0;
        int move_speed = 5;

        gui_element bg, button;

        public main_scene() {
            bg = new gui_element(new center_constraint(), new center_constraint(), new percentage_constraint(0.9f), new percentage_constraint(0.9f));
            bg.set_color(230, 221, 198);

            button = new gui_element(new percentage_constraint(0.1f), new percentage_constraint(0.1f), new aspect_constraint(1), new percentage_constraint(0.1f));
        }

        public override void update() {
            if(input.get_key_pressed(input.key_w)) {
                y -= move_speed;
            } else if(input.get_key_pressed(input.key_s)) {
                y += move_speed;
            }

            if(input.get_key_pressed(input.key_a)) {
                x -= move_speed;
            } else if(input.get_key_pressed(input.key_d)) {
                x += move_speed;
            }

            bg.update();
        }

        public override void render() {
            bg.render();

            SDL.SDL_Rect rect;
            rect.x = x;
            rect.y = y;
            rect.w = rect.h = 100;
            //draw.rect(game_manager.renderer, rect, 255, 255, 255, 255, true);
        }
    }
}