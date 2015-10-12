using UnityEngine;
using UnityEngine.UI.Windows;
using System.Collections.Generic;
using UnityEngine.UI.Windows.Extensions;
using UnityEngine.Extensions;

namespace UnityEngine.UI.Windows.Audio {

	public class Manager {

		private static Data currentData = new Data();

		public static void InitAndAdd(Data data, bool overrideValues = false) {

			data.SetupCache();
			
			Manager.currentData.Add(ClipType.Music, data, overrideValues);
			Manager.currentData.Add(ClipType.SFX, data, overrideValues);

		}

		public static void Change(WindowBase window, Source sourceInfo, ClipType clipType, int id, Audio.Window audioSettings) {

			var source = sourceInfo.GetSource(window, clipType, id);
			if (source == null) return;
			
			source.bypassEffects = audioSettings.bypassEffect;
			source.bypassListenerEffects = audioSettings.bypassListenerEffect;
			source.bypassReverbZones = audioSettings.bypassReverbEffect;
			source.loop = audioSettings.loop;
			
			source.priority = audioSettings.priority;
			source.volume = audioSettings.volume;
			source.pitch = audioSettings.pitch;
			source.panStereo = audioSettings.panStereo;
			source.spatialBlend = audioSettings.spatialBlend;
			source.reverbZoneMix = audioSettings.reverbZoneMix;

		}

		public static void Reset(AudioSource source) {
			
			source.bypassEffects = false;
			source.bypassListenerEffects = false;
			source.bypassReverbZones = false;
			source.loop = true;
			
			source.priority = 128;
			source.volume = 1f;
			source.pitch = 1f;
			source.panStereo = 0f;
			source.spatialBlend = 0f;
			source.reverbZoneMix = 1f;

		}

		public static void Play(WindowBase window, Source sourceInfo, ClipType clipType, int id) {
			
			var source = sourceInfo.GetSource(window, clipType, id);
			if (source == null) return;

			if (id == 0) {

				// Stop
				Manager.Stop(window, sourceInfo, clipType, id);
				return;

			}

			var state = Manager.currentData.GetState(clipType, id);
			if (state == null) {
				
				Manager.Stop(window, sourceInfo, clipType, id);
				return;

			}

			Manager.Reset(source);

			if (clipType == ClipType.Music) {

				source.clip = state.clip;
				source.Play();

			} else if (clipType == ClipType.SFX) {
				
				source.PlayOneShot(state.clip);

			}

		}
		
		public static void Stop(WindowBase window, Source sourceInfo, ClipType clipType, int id) {
			
			var source = sourceInfo.GetSource(window, clipType, id);
			if (source == null) return;

			source.Stop();
			source.clip = null;

		}

	}

}