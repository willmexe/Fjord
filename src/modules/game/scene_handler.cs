using System.Collections.Generic;
using System;
using System.Linq;
using System.IO;
using Fjord.Modules.Graphics;
using Fjord.Modules.Mathf;
using Fjord.Modules.Camera;
using Fjord.Modules.Tilemaps;
using Newtonsoft.Json;

namespace Fjord.Modules.Game {
    public abstract class scene {
        public string id = "";
        List<entity> entities = new List<entity>();
        public tilemap tiles;

        public void add_entity(entity e) {
            entities.Add(e);
        }

        public virtual void update() { foreach(entity e in entities) { e.update(); } }

        public virtual void render() { 
            if(!this.tiles.Equals(default(tilemap))) {
                for(var i = 0; i < tiles.grid_size.x; i++) {
                    for(var j = 0; j < tiles.grid_size.y; j++) {
                        V2 pos = new V2((int)(i * tiles.tile_size.x), (int)(j * tiles.tile_size.y));
                        draw.rect(new V4(pos.x, pos.y, tiles.tile_size.x, tiles.tile_size.y), color.black, false);

                        if(tiles.tile_map[i][j].Keys.ToList().Contains("tile_id")) {
                            if(tiles.tiles.Keys.ToList().Contains(tiles.tile_map[i][j]["tile_id"])) {
                                texture tile_texture = (texture)tiles.tiles[tiles.tile_map[i][j]["tile_id"]].tex.Clone();
                                draw.texture(pos, tile_texture);
                            }
                        }
                    }
                }
            }

            List<dynamic> sorted = new List<dynamic>();
            sorted.AddRange(entities);
            sorted.AddRange(draw.get_texture_buffer());
            sorted = sorted.OrderBy(e => e is entity ? e.depth : e.tex.get_depth()).ToList();
            foreach(dynamic e in sorted) {
                if(e is entity) {
                    Console.WriteLine("Hello");
                    e.render();
                } else {
                    draw.texture_direct(e.position, e.tex);
                }
            }

            // List<entity> sorted_entities = entities.OrderBy(e => e.depth).ToList();
            // foreach(entity e in entities) { 
            //     e.render(); 
            // } 

            // List<texture_buffer> sorted_texture_buffer = draw_texture_buffer.OrderBy(e => e.tex.get_depth()).ToList();
            // foreach(texture_buffer e in sorted_texture_buffer) { 
            //     texture_direct(e.position, e.tex);
            // } 

            draw.clean_texture_buffer();
        }
        public virtual void on_load() {}
        public virtual void on_unload() {}
    }

    public static class scene_handler {
        private static Dictionary<string, scene> scenes = new Dictionary<string, scene>();
        private static string current_scene; 
        private static int scenes_loaded = 0;

        public static void add_scene(string id, scene scene_add) {
            scenes.Add(id, scene_add);
        }
        
        public static void load_scene(string id) {
            if(current_scene != null)
                scenes[current_scene].on_unload();

            current_scene = id;
            
            try {
                scenes[current_scene].on_load();
            } catch(Exception e) {
                Debug.Debug.send("-- OnLoad Error --");
                game.stop(e);
            }

            scenes[current_scene].id = id;

            Debug.Debug.send("Loaded scene '" + id + "' successfully!");

            scenes_loaded++;
        }

        public static void load_tilemap(string id, string tilemap) {
            string JsonString = "";

            if(File.Exists(game.get_resource_folder() + "/" + game.asset_pack + "/data/tilemaps/" + tilemap + ".ftm")) {
                JsonString = File.ReadAllText(game.get_resource_folder() + "/" + game.asset_pack + "/data/tilemaps/" + tilemap + ".ftm");
            } else {
                Debug.Debug.send("Loading of tilemap '" + tilemap + "' failed!");
                return;
            }

            tilemap format = JsonConvert.DeserializeObject<tilemap>(JsonString);
            foreach(string key in format.tiles.Keys) {
                format.tiles[key].tex.set_texture(format.tiles[key].path);
            }
            
            scenes[id].tiles = format;
        }

        public static Dictionary<string, dynamic> get_tile(V2 pos) {
            V2 fixed_pos = new V2();
            fixed_pos.x = pos.x / scenes[current_scene].tiles.tile_size.x;
            fixed_pos.y = pos.y / scenes[current_scene].tiles.tile_size.y;

            if(!(fixed_pos.x >= 0 && fixed_pos.x < scenes[current_scene].tiles.grid_size.x)) {
                return scenes[current_scene].tiles.tile_map[0][0];
            }
            if(!(fixed_pos.y >= 0 && fixed_pos.y < scenes[current_scene].tiles.grid_size.y)) {
                return scenes[current_scene].tiles.tile_map[0][0];
            }

            return scenes[current_scene].tiles.tile_map[fixed_pos.x][fixed_pos.y];
        }

        public static void stop() {
            if(current_scene != null)
                scenes[current_scene].on_unload();
        }

        public static void update() {
            if(scenes.Count > 0) {
                scenes[current_scene].update();
            }
        }

        public static void render() {
            if(scenes.Count > 0) {
                scenes[current_scene].render();
            }
        }

        public static bool get_scene(string id) {
            return scenes.ContainsKey(id);
        }
    }
}