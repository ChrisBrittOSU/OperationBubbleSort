using UnityEngine;
using System.Collections;
namespace Lib{
	public enum TILE_T : int{
		// General types            XX
		PASSABLE =					0x00000001,
		SOLID = 						0x00000002,
		HAZARD =						0x00000004,

		// Solid Modifiers        XX
		STICKY = 						0x00000100,

		// Hazard types         XX
		SPIKE =							0x00010000
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
		public Map(int x, int y, int difficulty){
			m_x = x;
			m_y = y;
			generate(x,y,difficulty);
		}

		private TILE_T[,] map;
		private int m_x, m_y;

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
			for(int tx = 0; tx < x; ++tx){
				for(int ty = 0; ty < y; ++ty){
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

			removeSingleAround();

			fillAlmostFull();

			removeSoloPlatform();

			fillAlmostFull();
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
								for(int tx = 0; tx < m_x; ++tx){
									for(int ty = 0; ty < m_y; ++ty){
										if(checkFlag(tx,ty,TILE_T.SOLID)){
											file.Write("X");
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

		private void smoothPlatform(){
			for(int tx = 1; tx < m_x-1; ++tx){
				for(int ty = 1; ty < m_y-1; ++ty){
					if(checkFlag(tx,ty,TILE_T.PASSABLE)){
						int botCount = 0;
						for(int kx = tx - 1; kx <= tx + 1; ++kx){
							for(int ky = ty - 1; ky <= ty + 1; ++ky){
								if(kx == tx && ky == ty){
									continue;
								}
							}
						}
					}
				}
			}
		}
	}
}
