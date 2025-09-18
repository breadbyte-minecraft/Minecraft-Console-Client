﻿using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace MinecraftClient.Mapping
{
    /// <summary>
    /// Represents a Minecraft World
    /// </summary>
    public class World
    {
        /// <summary>
        /// The chunks contained into the Minecraft world
        /// Tuple<int, int>: Tuple<chunkX, chunkZ>
        /// </summary>
        private ConcurrentDictionary<Tuple<int, int>, ChunkColumn> chunks = new();

        /// <summary>
        /// The dimension info of the world
        /// </summary>
        private static Dimension curDimension= new();

        private static readonly Dictionary<string, Dimension> dimensionList = new();

        /// <summary>
        /// Chunk data parsing progress
        /// </summary>
        public int chunkCnt = 0;
        public int chunkLoadNotCompleted = 0;

        /// <summary>
        /// Read, set or unload the specified chunk column
        /// </summary>
        /// <param name="chunkX">ChunkColumn X</param>
        /// <param name="chunkZ">ChunkColumn Z</param>
        /// <returns>chunk at the given location</returns>
        public ChunkColumn? this[int chunkX, int chunkZ]
        {
            get
            {
                chunks.TryGetValue(new(chunkX, chunkZ), out ChunkColumn? chunkColumn);
                return chunkColumn;
            }
            set
            {
                Tuple<int, int> chunkCoord = new(chunkX, chunkZ);
                if (value == null)
                    chunks.TryRemove(chunkCoord, out _);
                else
                    chunks.AddOrUpdate(chunkCoord, value, (_, _) => value);
            }
        }


        /// <summary>
        /// Storage of all dimensional data - 1.19.1 and above
        /// </summary>
        /// <param name="registryCodec">Registry Codec nbt data</param>
        public static void StoreDimensionList(Dictionary<string, object> registryCodec)
        {
            
            if (!registryCodec.ContainsKey("minecraft:dimension_type")) {
                
                // If not, then we force the registry to be in the correct format
                if (registryCodec.ContainsKey("dimension_type")) {
                    
                    foreach (var key in registryCodec.Keys.ToArray()) {
                        // Skip entries with a namespace already
                        if (key.Contains(':', StringComparison.OrdinalIgnoreCase)) continue;
                        // Assume all other entries are in the minecraft namespace
                        registryCodec["minecraft:" + key] = registryCodec[key];
                        registryCodec.Remove(key);
                    }
                }
            }
            
            var dimensionListNbt = (object[])(((Dictionary<string, object>)registryCodec["minecraft:dimension_type"])["value"]);
            foreach (var (dimensionName, dimensionType) in from Dictionary<string, object> dimensionNbt in dimensionListNbt
                                                           let dimensionName = (string)dimensionNbt["name"]
                                                           let dimensionType = (Dictionary<string, object>)dimensionNbt["element"]
                                                           select (dimensionName, dimensionType))
            {
                StoreOneDimension(dimensionName, dimensionType);
            }
        }

        public static void LoadDefaultDimensions1206Plus()
        {
            // TODO: Move this to a JSON file.
            
            var defaultRegistryCodec = new Dictionary<string, object>
            {
                { "minecraft:dimension_type", new Dictionary<string, object>
                    {
                        { "value", new object[]
                            {
                                new Dictionary<string, object>
                                {
                                    { "name", "minecraft:overworld" },
                                    { "id", 0 },
                                    { "element", new Dictionary<string, object>
                                        {
                                            { "piglin_safe", (byte)0 },
                                            { "natural", 1 },
                                            { "ambient_light", 0.0 },
                                            { "monster_spawn_block_light_limit", 0 },
                                            { "infiniburn", "#minecraft:infiniburn_overworld" },
                                            { "respawn_anchor_works", 0 },
                                            { "has_skylight", 1 },
                                            { "bed_works", 1 },
                                            { "effects", "minecraft:overworld" },
                                            { "has_raids", 1 },
                                            { "logical_height", 384 },
                                            { "coordinate_scale", 1.0 },
                                            { "monster_spawn_light_level", new Dictionary<string, object>
                                                {
                                                    { "min_inclusive", 0 },
                                                    { "max_inclusive", 7 },
                                                    { "type", "minecraft:uniform" }
                                                }
                                            },
                                            { "min_y", -64 },
                                            { "ultrawarm", 0 },
                                            { "has_ceiling", 0 },
                                            { "height", 384 }
                                        }
                                    }
                                },
                                new Dictionary<string, object>
                                {
                                    { "name", "minecraft:overworld_caves" },
                                    { "id", 1 },
                                    { "element", new Dictionary<string, object>
                                        {
                                            { "piglin_safe", (byte)0 },
                                            { "natural", 1 },
                                            { "ambient_light", 0.0 },
                                            { "monster_spawn_block_light_limit", 0 },
                                            { "infiniburn", "#minecraft:infiniburn_overworld" },
                                            { "respawn_anchor_works", 0 },
                                            { "has_skylight", 1 },
                                            { "bed_works", 1 },
                                            { "effects", "minecraft:overworld" },
                                            { "has_raids", 1 },
                                            { "logical_height", 384 },
                                            { "coordinate_scale", 1.0 },
                                            { "monster_spawn_light_level", new Dictionary<string, object>
                                                {
                                                    { "min_inclusive", 0 },
                                                    { "max_inclusive", 7 },
                                                    { "type", "minecraft:uniform" }
                                                }
                                            },
                                            { "min_y", -64 },
                                            { "ultrawarm", 0 },
                                            { "has_ceiling", 1 },
                                            { "height", 384 }
                                        }
                                    }
                                },
                                new Dictionary<string, object>
                                {
                                    { "name", "minecraft:the_end" },
                                    { "id", 2 },
                                    { "element", new Dictionary<string, object>
                                        {
                                            { "piglin_safe", (byte)0 },
                                            { "natural", 0 },
                                            { "ambient_light", 0.0 },
                                            { "monster_spawn_block_light_limit", 0 },
                                            { "infiniburn", "#minecraft:infiniburn_end" },
                                            { "respawn_anchor_works", 0 },
                                            { "has_skylight", 0 },
                                            { "bed_works", 0 },
                                            { "effects", "minecraft:the_end" },
                                            { "fixed_time", 6000 },
                                            { "has_raids", 1 },
                                            { "logical_height", 256 },
                                            { "coordinate_scale", 1.0 },
                                            { "monster_spawn_light_level", new Dictionary<string, object>
                                                {
                                                    { "min_inclusive", 0 },
                                                    { "max_inclusive", 7 },
                                                    { "type", "minecraft:uniform" }
                                                }
                                            },
                                            { "min_y", 0 },
                                            { "ultrawarm", 0 },
                                            { "has_ceiling", 0 },
                                            { "height", 256 }
                                        }
                                    }
                                },
                                new Dictionary<string, object>
                                {
                                    { "name", "minecraft:the_nether" },
                                    { "id", 3 },
                                    { "element", new Dictionary<string, object>
                                        {
                                            { "piglin_safe", (byte)1 },
                                            { "natural", 0 },
                                            { "ambient_light", 0.1 },
                                            { "monster_spawn_block_light_limit", 15 },
                                            { "infiniburn", "#minecraft:infiniburn_nether" },
                                            { "respawn_anchor_works", 1 },
                                            { "has_skylight", 0 },
                                            { "bed_works", 0 },
                                            { "effects", "minecraft:the_nether" },
                                            { "fixed_time", 18000 },
                                            { "has_raids", 0 },
                                            { "logical_height", 128 },
                                            { "coordinate_scale", 8.0 },
                                            { "monster_spawn_light_level", 7 },
                                            { "min_y", 0 },
                                            { "ultrawarm", 1 },
                                            { "has_ceiling", 1 },
                                            { "height", 256 }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            };

            StoreDimensionList(defaultRegistryCodec);
        }

        /// <summary>
        /// Store one dimension - Directly used in 1.16.2 to 1.18.2
        /// </summary>
        /// <param name="dimensionName">Dimension name</param>
        /// <param name="dimensionType">Dimension Type nbt data</param>
        public static void StoreOneDimension(string dimensionName, Dictionary<string, object> dimensionType)
        {
            if (dimensionList.ContainsKey(dimensionName))
                dimensionList.Remove(dimensionName);
            dimensionList.Add(dimensionName, new Dimension(dimensionName, dimensionType));
        }


        /// <summary>
        /// Set current dimension - 1.16 and above
        /// </summary>
        /// <param name="name">	The name of the dimension type</param>
        /// <param name="nbt">The dimension type (NBT Tag Compound)</param>
        public static void SetDimension(string name)
        {
            try
            {
                curDimension = dimensionList[name]; // Should not fail
            }
            catch (KeyNotFoundException)
            {
                // For newer versions, assume that we are dealing with the default namespace
                curDimension = dimensionList["minecraft:" + name];
            }
        }

        /// <summary>
        /// Get current dimension
        /// </summary>
        /// <returns>Current dimension</returns>
        public static Dimension GetDimension()
        {
            return curDimension;
        }

        /// <summary>
        /// Set chunk column at the specified location
        /// </summary>
        /// <param name="chunkX">ChunkColumn X</param>
        /// <param name="chunkY">ChunkColumn Y</param>
        /// <param name="chunkZ">ChunkColumn Z</param>
        /// <param name="chunkColumnSize">ChunkColumn size</param>
        /// <param name="chunk">Chunk data</param>
        /// <param name="loadCompleted">Whether the ChunkColumn has been fully loaded</param>
        public void StoreChunk(int chunkX, int chunkY, int chunkZ, int chunkColumnSize, Chunk? chunk, bool loadCompleted)
        {
            ChunkColumn chunkColumn = chunks.GetOrAdd(new(chunkX, chunkZ), (_) => new(chunkColumnSize));
            chunkColumn[chunkY] = chunk;
            if (loadCompleted)
                chunkColumn.FullyLoaded = true;
        }

        /// <summary>
        /// Get chunk column at the specified location
        /// </summary>
        /// <param name="location">Location to retrieve chunk column</param>
        /// <returns>The chunk column</returns>
        public ChunkColumn? GetChunkColumn(Location location)
        {
            return this[location.ChunkX, location.ChunkZ];
        }

        /// <summary>
        /// Get block at the specified location
        /// </summary>
        /// <param name="location">Location to retrieve block from</param>
        /// <returns>Block at specified location or Air if the location is not loaded</returns>
        public Block GetBlock(Location location)
        {
            ChunkColumn? column = GetChunkColumn(location);
            if (column != null)
            {
                Chunk? chunk = column.GetChunk(location);
                if (chunk != null)
                    return chunk.GetBlock(location);
            }
            return Block.Air;
        }

        /// <summary>
        /// Look for a block around the specified location
        /// </summary>
        /// <param name="from">Start location</param>
        /// <param name="block">Block type</param>
        /// <param name="radius">Search radius - larger is slower: O^3 complexity</param>
        /// <returns>Block matching the specified block type</returns>
        public List<Location> FindBlock(Location from, Material block, double radius)
        {
            return FindBlock(from, block, radius, radius, radius);
        }

        /// <summary>
        /// Look for a block around the specified location
        /// </summary>
        /// <param name="from">Start location</param>
        /// <param name="block">Block type</param>
        /// <param name="radiusx">Search radius on the X axis</param>
        /// <param name="radiusy">Search radius on the Y axis</param>
        /// <param name="radiusz">Search radius on the Z axis</param>
        /// <returns>Block matching the specified block type</returns>
        public List<Location> FindBlock(Location from, Material block, double radiusx, double radiusy, double radiusz)
        {
            Location minPoint = new Location(from.X - radiusx, from.Y - radiusy, from.Z - radiusz);
            Location maxPoint = new Location(from.X + radiusx, from.Y + radiusy, from.Z + radiusz);

            List<int> xRange = Enumerable.Range(Convert.ToInt32(Math.Floor(minPoint.X)), Convert.ToInt32(Math.Floor(maxPoint.X - minPoint.X)) + 1).ToList();
            List<int> yRange = Enumerable.Range(Convert.ToInt32(Math.Floor(minPoint.Y)), Convert.ToInt32(Math.Floor(maxPoint.Y - minPoint.Y)) + 1).ToList();
            List<int> zRange = Enumerable.Range(Convert.ToInt32(Math.Floor(minPoint.Z)), Convert.ToInt32(Math.Floor(maxPoint.Z - minPoint.Z)) + 1).ToList();

            List<Location> listOfBlocks = xRange.SelectMany(x => yRange.SelectMany(y => zRange.Select(z => new Location(x, y, z)))).ToList();

            return listOfBlocks.Where(loc => GetBlock(loc).Type == block).ToList();
        }

        /// <summary>
        /// Set block at the specified location
        /// </summary>
        /// <param name="location">Location to set block to</param>
        /// <param name="block">Block to set</param>
        public void SetBlock(Location location, Block block)
        {
            ChunkColumn? column = this[location.ChunkX, location.ChunkZ];
            if (column != null && column.ColumnSize >= location.ChunkY)
            {
                Chunk? chunk = column.GetChunk(location);
                if (chunk == null)
                    column[location.ChunkY] = chunk = new Chunk();
                chunk[location.ChunkBlockX, location.ChunkBlockY, location.ChunkBlockZ] = block;
            }
        }

        /// <summary>
        /// Clear all terrain data from the world
        /// </summary>
        public void Clear()
        {
            chunks = new();
            chunkCnt = 0;
            chunkLoadNotCompleted = 0;
        }

        public static string GetChunkLoadingStatus(World world)
        {
            double chunkLoadedRatio;
            if (world.chunkCnt == 0)
                chunkLoadedRatio = 0;
            else
                chunkLoadedRatio = (world.chunkCnt - world.chunkLoadNotCompleted) / (double)world.chunkCnt;

            string status = string.Format(Translations.cmd_move_chunk_loading_status,
                    chunkLoadedRatio, world.chunkCnt - world.chunkLoadNotCompleted, world.chunkCnt);

            return status;
        }
    }
}
