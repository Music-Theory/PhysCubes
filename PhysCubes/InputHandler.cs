namespace PhysCubes {
	using System;
	using System.Collections.Generic;
	using System.Numerics;
	using OpenGL;
	using ReturnToGL.Physics;
	using ReturnToGL.Rendering;
	using SDL2;

	public class InputHandler {

		List<SDL.SDL_Keycode> pressedKeys = new List<SDL.SDL_Keycode>();
		bool mouse1Down = false;

		public void Update() {
			while (SDL.SDL_PollEvent(out SDL.SDL_Event e) > 0) {
				switch (e.type) {
					case SDL.SDL_EventType.SDL_QUIT:
						Program.run = false;
						break;
					case SDL.SDL_EventType.SDL_KEYDOWN:
						if (!pressedKeys.Contains(e.key.keysym.sym)) {
							pressedKeys.Add(e.key.keysym.sym);
						}
						break;
					case SDL.SDL_EventType.SDL_KEYUP:
						pressedKeys.Remove(e.key.keysym.sym);
						break;
					case SDL.SDL_EventType.SDL_MOUSEBUTTONDOWN:
						mouse1Down = e.button.button == 1;
						break;
					case SDL.SDL_EventType.SDL_MOUSEBUTTONUP:
						if (mouse1Down && e.button.button == 1) {
							mouse1Down = false;
						}
						break;
					case SDL.SDL_EventType.SDL_WINDOWEVENT:
						HandleWindowEvent(e.window);
						break;
				}
			}
			UpdateKeys();
			UpdateMouse();
		}

		ushort mouseFrames = 0;

		void UpdateMouse() {
			if (mouse1Down && mouseFrames > 15) {
				mouseFrames = 0;
				SDL.SDL_GetMouseState(out int x, out int y);
				Program.SpawnBox(x, y);
			} else { mouseFrames++; }
		}

		void UpdateKeys() {
			foreach (SDL.SDL_Keycode key in pressedKeys) {
				switch (key) {
					case SDL.SDL_Keycode.SDLK_DELETE:
						for (int i = Physics.boxes.Count - 1; i > 1; i--) { Physics.boxes.RemoveAt(i); }
						break;
					case SDL.SDL_Keycode.SDLK_r:
						Program.Reset();
						break;
					case SDL.SDL_Keycode.SDLK_SPACE:
						Physics.ResetBoxes();
						break;
					case SDL.SDL_Keycode.SDLK_t:
						Program.SwapTex();
						break;
					case SDL.SDL_Keycode.SDLK_l:
						Program.ToggleReference();
						break;
					case SDL.SDL_Keycode.SDLK_p:
						Program.Pause();
						break;
					case SDL.SDL_Keycode.SDLK_KP_0:
						Program.Reset();
						Program.cam.Position = Vector3.Zero;
						break;
					case SDL.SDL_Keycode.SDLK_w:
						Program.cam.Move(0, 0, 1);
						break;
					case SDL.SDL_Keycode.SDLK_a:
						Program.cam.Move(-1, 0, 0);
						break;
					case SDL.SDL_Keycode.SDLK_s:
						Program.cam.Move(0, 0, -1);
						break;
					case SDL.SDL_Keycode.SDLK_d:
						Program.cam.Move(1, 0, 0);
						break;
					case SDL.SDL_Keycode.SDLK_q:
						Program.cam.Move(0, 1, 0);
						break;
					case SDL.SDL_Keycode.SDLK_e:
						Program.cam.Move(0, -1, 0);
						break;
					case SDL.SDL_Keycode.SDLK_KP_8:
						Program.cam.Rotate(1, 0, 0);
						break;
					case SDL.SDL_Keycode.SDLK_KP_2:
						Program.cam.Rotate(-1, 0, 0);
						break;
					case SDL.SDL_Keycode.SDLK_KP_4:
						Program.cam.Rotate(0, 1, 0);
						break;
					case SDL.SDL_Keycode.SDLK_KP_6:
						Program.cam.Rotate(0, -1, 0);
						break;
					case SDL.SDL_Keycode.SDLK_KP_7:
						Program.cam.Rotate(0, 0, 1);
						break;
					case SDL.SDL_Keycode.SDLK_KP_9:
						Program.cam.Rotate(0, 0, -1);
						break;
				}
			}
			if (pressedKeys.Contains(SDL.SDL_Keycode.SDLK_t)) {
				pressedKeys.Remove(SDL.SDL_Keycode.SDLK_t);
			}
			if (pressedKeys.Contains(SDL.SDL_Keycode.SDLK_r)) {
				pressedKeys.Remove(SDL.SDL_Keycode.SDLK_r);
			}
			if (pressedKeys.Contains(SDL.SDL_Keycode.SDLK_DELETE)) {
				pressedKeys.Remove(SDL.SDL_Keycode.SDLK_DELETE);
			}
			if (pressedKeys.Contains(SDL.SDL_Keycode.SDLK_KP_0)) {
				pressedKeys.Remove(SDL.SDL_Keycode.SDLK_KP_0);
			}
			if (pressedKeys.Contains(SDL.SDL_Keycode.SDLK_p)) {
				pressedKeys.Remove(SDL.SDL_Keycode.SDLK_p);
			}
			if (pressedKeys.Contains(SDL.SDL_Keycode.SDLK_l)) {
				pressedKeys.Remove(SDL.SDL_Keycode.SDLK_l);
			}
		}

		void HandleWindowEvent(SDL.SDL_WindowEvent e) {
			switch (e.windowEvent) {
				case SDL.SDL_WindowEventID.SDL_WINDOWEVENT_FOCUS_LOST:
					pressedKeys.Clear();
					mouse1Down = false;
					break;
				case SDL.SDL_WindowEventID.SDL_WINDOWEVENT_RESIZED:
					OnResized(e);
					break;
				default:
					throw new ArgumentOutOfRangeException();
			}
		}

		void OnResized(SDL.SDL_WindowEvent e) {
			Program.projMat = Matrix4x4.CreatePerspectiveFieldOfView(.45f, (float) e.data1 / e.data2, .1f, 1000f);

			Program.res = new Vector<int>(new []{e.data1, e.data2});

			// this is assuming that y < x
			RenderText.textSquareFactor = new Vector2(e.data2 / e.data1, 1);

			Program.UpdateModelView();
			Program.cam.Refresh();

			Gl.Viewport(0, 0, e.data1, e.data2);
		}

	}
}