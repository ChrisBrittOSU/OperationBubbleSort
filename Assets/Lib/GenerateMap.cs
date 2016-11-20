using UnityEngine;
using System.Collections;
namespace Lib{
	public enum TILE_T : int{
		NULL = 							0x00000000,
		// General types            XX
		PASSABLE =					0x00000001,
		SOLID = 						0x00000002,
		HAZARD =						0x00000004,
		SPAWN_POINT =				0x00000008,
		EXIT_POINT =				0x00000010,

		// Solid Modifiers        XX
		STICKY = 						0x00000100,
		SLIPPERY =					0x00000200,

		// Hazard types         XX
		SPIKE =							0x00010000,
		SCORE =							0x00020000
	};

	public enum FACE_T : int{
		NULL = 0x0,
		RIGHT = 0x1,
		LEFT = 0x2,
		DOWN = 0x4,
		UP = 0x8
	};

	public class RandomTile{
		private int current = 0;
		private int maxDif = 5;
		private int minVal = 3, maxVal = 6;

		private System.Random rgen = new System.Random();
		public int getChoice(){
			if(current == 0){
				current = rgen.Next()%(maxDif) - 2;
			}
			else if(current < -minVal){
				current += rgen.Next()%(maxDif+2);
			}
			else if(current > maxVal){
				current -= rgen.Next()%(maxDif+2);
			}
			else if(current < 0){
				current -= rgen.Next()%(maxDif) - maxDif/2;
			}
			else if(current > 0){
				current += rgen.Next()%(maxDif) - maxDif/2;
			}

			return current == 0 ? getChoice() : current / Mathf.Abs(current);
			// return current;
		}
	}

	public class Map{
		public Map(int x, int y, float difficulty){
			m_x = x;
			m_y = y;
			generate(x,y,difficulty);
		}

		private TILE_T[,] map;
		private int m_x, m_y;
		private System.Random rgen = new System.Random();

		public TILE_T at(int x, int y){
			if(x > m_x){
				throw new System.IndexOutOfRangeException("x:"+x+" > maxX"+m_x);
			}
			if(y > m_y){
				throw new System.IndexOutOfRangeException("y:"+y+" > maxY"+m_y);
			}

			return map[x,y];
		}

		private bool inRange(int x, int y){
			return x < m_x && y < m_y && x >= 0 && y >= 0;
		}

		public bool checkFlag(int x, int y, TILE_T flag){
			return (map[x,y] & flag) == flag;
		}

		public bool softCheckFlag(int x, int y, TILE_T flag){
			return (map[x,y] & flag) > 0;
		}

		private int countAround(int x, int y, TILE_T flag){
			int count = 0;
			for(int tx = x - 1; tx <= x + 1; ++tx){
				for(int ty = y - 1; ty <= y + 1; ++ty){
					if(tx == x && ty == y){
						continue;
					}
					else if(!inRange(tx, ty)){
						continue;
					} else if(checkFlag(tx,ty,flag)){
						++count;
					}
				}
			}
			return count;
		}

		private bool isAlone(int x, int y, TILE_T flag){
			for(int tx = x - 1; tx <= x + 1; ++tx){
				for(int ty = y - 1; ty <= y + 1; ++ty){
					if(tx == x && ty == y){
						continue;
					}
					else if(!inRange(tx, ty)){
						continue;
					} else if(checkFlag(tx,ty,flag)){
						return false;
					}
				}
			}
		return true;
		}

		// Generate a map of the given size.
		public void generate(int x, int y, float difficulty){
			m_x = x;
			m_y = y;

			map = new TILE_T[x,y];

			RandomTile rt = new RandomTile();

			// Lets do a solid pass for the first time:
			// We are going to make a solid border around the side
			// We are going to with a density factor based on the difficulty
			for(int ty = 0; ty < y; ++ty){
				for(int tx = 0; tx < x; ++tx){
					if(tx==0||ty==0||tx==x-1||ty==y-1){
						map[tx,ty] |= TILE_T.SOLID;
					}
					// Resort to the random generator
					else{
						int choice = rt.getChoice();
						if(choice < 0){
							map[tx,ty] = TILE_T.SOLID;
						}else{
							map[tx,ty] = TILE_T.PASSABLE;
						}
					}
				}
			}

			removeSoloPlatform();

			removeDiagonal();

			removeSoloPlatform();

			removeSingleAround();

			fillAlmostFull();

			removeSoloPlatform();

			fillAlmostFull();

			smoothPlatform(TILE_T.PASSABLE, TILE_T.SOLID);
			smoothPlatform(TILE_T.SOLID, TILE_T.PASSABLE);

			smoothSides(TILE_T.PASSABLE, TILE_T.SOLID);
			smoothSides(TILE_T.SOLID, TILE_T.PASSABLE);

			smoothPlatform(TILE_T.PASSABLE, TILE_T.SOLID);
			smoothPlatform(TILE_T.SOLID, TILE_T.PASSABLE);

			smoothSides(TILE_T.PASSABLE, TILE_T.SOLID);
			smoothSides(TILE_T.SOLID, TILE_T.PASSABLE);

			removeDiagonal();

			cleanupFlag(TILE_T.PASSABLE);
			cleanupFlag(TILE_T.SOLID);

			placeTraps(difficulty);

			bottomTraps(difficulty);

			placeSpawnPoint();
			placeExitPoint();

			seedSolid(difficulty);
			splatterSolid(difficulty);


			if(checkUpperPath()){
				Debug.Log("We have done checkUpperPath() successfully");
				if(manageCheckForPath()){
					Debug.Log("We have gone and successfully verified that we have a valid path. ");
				} else {
					Debug.LogError("We do not have a valid path, going to have to try again. ");
					generate(x, y, difficulty);
				}
			} else{
				Debug.LogError("We need to try again, checkUpperPath() failed");
				generate(x, y, difficulty);
			}
			Debug.Log("Done with generating the map");
		}

		public void printFile(string filename){
			if(System.IO.File.Exists(filename))
			{
					// Use a try block to catch IOExceptions, to
					// handle the case of the file already being
					// opened by another process.
					try
					{
							System.IO.File.Delete(filename);
					}
					catch (System.IO.IOException e)
					{
							Debug.LogError(e.Message);
							return;
					}
			}
			using (System.IO.StreamWriter file =
					new System.IO.StreamWriter(filename, true))
							{
								for(int ty = 0; ty < m_y; ++ty){
									for(int tx = 0; tx < m_x; ++tx){
										if(softCheckFlag(tx,ty,TILE_T.SOLID)){
											if(softCheckFlag(tx,ty,TILE_T.STICKY)){
												file.Write("W");
											} else if(softCheckFlag(tx,ty,TILE_T.SLIPPERY)){
												file.Write("O");
											} else {
												file.Write("X");
											}
										} else if(checkFlag(tx, ty, TILE_T.SPAWN_POINT)){
											file.Write("S");
										} else if(checkFlag(tx, ty, TILE_T.EXIT_POINT)){
											file.Write("E");
										} else if(softCheckFlag(tx, ty, TILE_T.HAZARD)){
											file.Write("#");
										} else{
											file.Write(" ");
										}
									}
									file.Write("\n");
								}
							}
		}

		public int maxX(){
			return m_x;
		}

		public int maxY(){
			return m_y;
		}

		public FACE_T getFace(int x, int y){
			FACE_T result = FACE_T.NULL;
			if(inRange(x, y-1)&&(checkFlag(x, y-1, TILE_T.SOLID))){
				result |= FACE_T.UP;
			}
			if(inRange(x, y+1)&&(checkFlag(x, y+1, TILE_T.SOLID))){
				result |= FACE_T.DOWN;
			}
			if(inRange(x+1, y)&&(checkFlag(x+1, y, TILE_T.SOLID))){
				result |= FACE_T.RIGHT;
			}
			if(inRange(x-1, y)&&(checkFlag(x-1, y, TILE_T.SOLID))){
				result |= FACE_T.LEFT;
			}
			return result;
		}

		// Remove solo platforms
		private void removeSoloPlatform(){
			for(int tx = 1; tx < m_x-1; ++tx){
				for(int ty = 1; ty < m_y-1; ++ty){
					if(isAlone(tx,ty,TILE_T.SOLID)){
						map[tx,ty] = TILE_T.PASSABLE;
					}
				}
			}
		}

		// Remove outsiders to give us some more Space
		private void removeSingleAround(){
			for(int tx = 1; tx < m_x-1; ++tx){
				for(int ty = 1; ty < m_y-1; ++ty){
					if(countAround(tx,ty,TILE_T.SOLID)<=1){
						map[tx,ty] = TILE_T.PASSABLE;
					}
				}
			}
		}

		private void seedSolid(float difficulty){
			int count = (int)(difficulty * Mathf.Log(difficulty, 2));
			for(int k = 0; k < count; ++k){
				int tx = rgen.Next()%(m_x-2) + 1;
				int ty = rgen.Next()%(m_y-2) + 1;
				if(checkFlag(tx, ty, TILE_T.SOLID)){
					switch(rgen.Next()%2){
						case 0:
							map[tx, ty] |= TILE_T.STICKY;
							break;
						case 1:
							map[tx, ty] |= TILE_T.SLIPPERY;
							break;
						default:
							Debug.LogError("seedSolid() escaped its switch");
							break;
					}
				}
			}
		}

		private void splatterSolid(float difficulty){
			int rate = 0;
			int pass = 1;
			if(difficulty >= 25f){
				do{
					rate += rgen.Next()%70;
					TILE_T[,] vals = new TILE_T[m_x, m_y];
					Debug.Log("splatterSolid() going for the pass #"+pass+": "+rate+" / "+difficulty);
					for(int tx = 1; tx < m_x-1; ++tx){
						for(int ty = 1; ty < m_y-1; ++ty){
							if(softCheckFlag(tx, ty, TILE_T.STICKY|TILE_T.SLIPPERY)){
								for(int kx = tx - 1; kx <= tx + 1; ++kx){
									for(int ky = ty - 1; ky <= ty + 1; ++ky){
										if(checkFlag(kx, ky, TILE_T.SOLID)){
											vals[kx, ky] = map[tx, ty];
										}
									}
								}
							}
						}
					}

					// Now go and update the map
					for(int tx = 0; tx < m_x; ++tx){
						for(int ty = 0; ty < m_y; ++ty){
							map[tx, ty] |= vals[tx, ty];
						}
					}
				}while(rate < difficulty);
			} else {
				Debug.Log("Difficulty is to low for splatterSolid(): "+ difficulty + " >= " + 25f);
			}
		}

		private void removeDiagonal(){
			for(int tx = 1; tx < m_x - 2; ++tx){
				for(int ty = 1; ty < m_y - 2; ++ty){
					if(
							(
								checkFlag(tx,ty,TILE_T.SOLID) &&
								checkFlag(tx+1,ty+1,TILE_T.SOLID) &&
								checkFlag(tx+1,ty,TILE_T.PASSABLE) &&
								checkFlag(tx,ty+1,TILE_T.PASSABLE)
							)
							||
							(
								checkFlag(tx,ty,TILE_T.PASSABLE) &&
								checkFlag(tx+1,ty+1,TILE_T.PASSABLE) &&
								checkFlag(tx+1,ty,TILE_T.SOLID) &&
								checkFlag(tx,ty+1,TILE_T.SOLID)
							)
						){
							map[tx,ty] = TILE_T.PASSABLE;
							map[tx+1,ty] = TILE_T.PASSABLE;
							map[tx,ty+1] = TILE_T.PASSABLE;
							map[tx+1,ty+1] = TILE_T.PASSABLE;
						}
				}
			}
		}

		private void placeTraps(float difficulty){
			int kernelSize = 10;
			for(int kx = 0; kx < m_x / kernelSize - 1; ++kx){
				for(int ky = 0; ky < m_y / kernelSize - 1; ++ky){
					if(rgen.Next()%(100) <= difficulty){
						// We are going to place traps
						int trapCounter = 0;
						do {
							trapCounter += rgen.Next()%50;
							bool isPlaced = false;
							for(int k = 0; k < kernelSize * 2 && !isPlaced; ++k){
								int tx = kx*kernelSize + rgen.Next()%kernelSize;
								int ty = ky*kernelSize + rgen.Next()%kernelSize;
								if(checkFlag(tx,ty,TILE_T.PASSABLE)){
									isPlaced = true;
									map[tx, ty] = TILE_T.HAZARD;
									switch(rgen.Next()%1){
										case 0:
											map[tx, ty] |= TILE_T.SPIKE;
											break;
										default:
											Debug.LogError("ERROR ~ Trap Selection Escaped");
											break;
									}
								}
							}
						}while(trapCounter < difficulty);
					}
				}
			}
		}

		// Fill in "almost full"
		private void fillAlmostFull(){
			for(int tx = 1; tx < m_x-1; ++tx){
				for(int ty = 1; ty < m_y-1; ++ty){
					if(countAround(tx,ty,TILE_T.SOLID) >= 6){
						map[tx,ty] = TILE_T.SOLID;
					}
				}
			}
		}

		private bool checkUpperPath(){
			for(int tx = 2; tx < m_x - 2; ++tx){
				bool isOpen = false;
				for(int ty = 0; ty < m_y; ++ty){
					isOpen = checkFlag(tx, ty, TILE_T.PASSABLE) ? true : isOpen;
				}
				if(!isOpen) return false;
			}
			return true;
		}

		private bool manageCheckForPath(){
			// Find the spawn point flag.
			bool spawnFound = false, exitFound = false;
			for(int tx = 0; tx < m_x; ++tx){
				for(int ty = 0; ty < m_y; ++ty){
					if(checkFlag(tx, ty, TILE_T.SPAWN_POINT)){
						spawnFound = true;
					} else if(checkFlag(tx, ty, TILE_T.EXIT_POINT)){
						exitFound = true;
					}
				}
			}
			if(spawnFound) Debug.Log("Spawn is found");
			if(exitFound) Debug.Log("Exit is found");

			for(int ty = 4; ty < m_y - 4; ++ty){
				bool isOpen = false;
				for(int tx = 4; tx < m_x - 4; ++tx){
					isOpen = (
						checkFlag(tx, ty-1, TILE_T.PASSABLE) &&
						checkFlag(tx, ty, TILE_T.PASSABLE) &&
						checkFlag(tx, ty+1, TILE_T.PASSABLE)
					) ? true : isOpen;
				}
				if(!isOpen) {
					Debug.LogError("manageCheckForPath(), no veritical route at: ty = "+ty);
					return false;
				}
			}
			// return checkForPath(spawnX, spawnY, array, 0);
			return spawnFound && exitFound;
		}

		private void bottomTraps(float difficulty){
			if(difficulty<=0.001f) return;

			int y = m_y - 2;
			for(int x = 0; x < m_x; ++x){
				if(checkFlag(x, y, TILE_T.PASSABLE)){
					if(difficulty >= 50f){
						map[x,y] = TILE_T.HAZARD|TILE_T.SPIKE;
					} else if(difficulty >= 25f){
						map[x,y] = rgen.Next()%2 == 0 ? TILE_T.HAZARD|TILE_T.SPIKE : map[x,y];
					} else {
						map[x,y] = rgen.Next()%4 == 0 ? TILE_T.HAZARD|TILE_T.SPIKE : map[x,y];
					}
				}
			}
		}

		private bool placeExitPoint(){
			for(int ty = 5; ty < m_y - 6; ++ty){
				for(int tx = m_x - 7; tx > 2; --tx){
					if(countAround(tx, ty, TILE_T.PASSABLE)>=8){
						map[tx, ty] = TILE_T.EXIT_POINT;
						map[tx-2, ty+1] = TILE_T.SOLID;
						map[tx-1, ty+1] = TILE_T.SOLID;
						map[tx, ty+1] = TILE_T.SOLID;
						map[tx+1, ty+1] = TILE_T.SOLID;
						map[tx+2, ty+1] = TILE_T.SOLID;
						return true;
					}
				}
			}
			return false;
		}

		private bool placeSpawnPoint(){
			for(int ty = m_y - 7; ty > 2; --ty){
				for(int tx = 5; tx < m_x; ++tx){
					if(countAround(tx, ty, TILE_T.PASSABLE)>=8){
						map[tx, ty] = TILE_T.SPAWN_POINT;
						map[tx-2, ty+1] = TILE_T.SOLID;
						map[tx-1, ty+1] = TILE_T.SOLID;
						map[tx, ty+1] = TILE_T.SOLID;
						map[tx+1, ty+1] = TILE_T.SOLID;
						map[tx+2, ty+1] = TILE_T.SOLID;
						return false;
					}
				}
			}
			return false;
		}

		private void cleanupFlag(TILE_T flag){
			for(int tx = 0; tx < m_x; ++tx){
				for(int ty = 0; ty < m_y; ++ty){
					if(softCheckFlag(tx, ty, flag)){
						map[tx, ty] = flag;
					}
				}
			}
		}

		private void smoothPlatform(TILE_T old, TILE_T to){
			for(int tx = 1; tx < m_x-1; ++tx){
				for(int ty = 1; ty < m_y-1; ++ty){
					if(softCheckFlag(tx,ty,old)){
						int botCount = 0;
						int topCount = 0;
						int sideCount = 0;
						for(int kx = tx - 1; kx <= tx + 1; ++kx){
							for(int ky = ty - 1; ky <= ty + 1; ++ky){
								if(kx == tx && ky == ty){
									continue;
								}
								// Check sides
								else if(ky == ty){
									if(softCheckFlag(kx, ky, to)){
										++sideCount;
									}
								}
								else if(ky == ty - 1){
									if(softCheckFlag(kx,ky,to)){
										++topCount;
									}
								}
								else if(ky == ty + 1){
									if(softCheckFlag(kx,ky,to)){
										++botCount;
									}
								}
							}
						}

						// It must be surrounded to be valid
						if(sideCount==2){
							if(botCount >= 3 || sideCount >= 3 || (botCount + sideCount)>=5){
								map[tx,ty] = to;
							}
						}
					}
				}
			}
		}

		private void smoothSides(TILE_T old, TILE_T to){
			for(int tx = 1; tx < m_x-1; ++tx){
				for(int ty = 1; ty < m_y-1; ++ty){
					if(softCheckFlag(tx,ty,old)){
						int botCount = 0;
						int topCount = 0;
						int sideCount = 0;
						for(int kx = tx - 1; kx <= tx + 1; ++kx){
							for(int ky = ty - 1; ky <= ty + 1; ++ky){
								if(kx == tx && ky == ty){
									continue;
								}
								// Check sides
								else if(kx == tx){
									if(softCheckFlag(kx, ky, to)){
										++sideCount;
									}
								}
								else if(kx == tx - 1){
									if(softCheckFlag(kx,ky,to)){
										++topCount;
									}
								}
								else if(kx == tx + 1){
									if(softCheckFlag(kx,ky,to)){
										++botCount;
									}
								}
							}
						}

						// It must be surrounded to be valid
						if(sideCount==2){
							if(botCount >= 3 || sideCount >= 3 || (botCount + sideCount)>=5){
								map[tx,ty] = to;
							}
						}
					}
				}
			}
		}
	}
}
