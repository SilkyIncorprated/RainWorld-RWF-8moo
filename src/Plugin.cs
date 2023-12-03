using System;
using BepInEx;
using UnityEngine;
using RWF;
using RWF.Swagshit;
using System.IO;
using System.Collections.Generic;
using RWF.FNFJSON;
using System.Linq;
using System.Collections;

namespace EightMoo
{
    [BepInPlugin(MOD_ID, "RWF: 8moo", "1.0.0")]
    class Plugin : BaseUnityPlugin
    {
        private const string MOD_ID = "silky.rwf.8moons";

        /*var chars = [
            "char_jank" => jank,
                    "char_RBLXCYC" => RBLXCYC,
                    "char_krollge" => krollge,
                    "char_zomb" => zomb
        ];*/

        private Dictionary<string, Character> chars = new Dictionary<string, Character>();

        private Character currentSingerTarget = null;


        // Add hooks
        public void OnEnable()
        {
            On.RainWorld.OnModsInit += Extras.WrapInit(LoadResources);

            FunkinMenu.OnStepHit += FunkinMenu_OnStepHit;
            FunkinMenu.OnCreate += FunkinMenu_OnCreate;
            FunkinMenu.OnPlayerHit += FunkinMenu_OnPlayerHit;
            FunkinMenu.OnBeatHit += FunkinMenu_OnBeatHit;
            FunkinMenu.UpdateCameraTarget += FunkinMenu_UpdateCameraTarget;
        }

        private void FunkinMenu_UpdateCameraTarget(FunkinMenu self)
        {
            if (self.SONG.Name != "joner") return;

            var boyfriendlass = currentSingerTarget.flipped ? currentSingerTarget.pos + new UnityEngine.Vector2(-currentSingerTarget.CameraOffset.x, currentSingerTarget.CameraOffset.y) : currentSingerTarget.pos + new UnityEngine.Vector2(currentSingerTarget.CameraOffset.x, currentSingerTarget.CameraOffset.y);

            if (Conductor.curBeat >= 0 && self.SONG.Sections.Count > Conductor.curSection)
            {
                if (self.SONG.Sections[Conductor.curSection].mustHitSection)
                    self.cameraTarget = boyfriendlass;
            }

        }

        private void FunkinMenu_OnBeatHit(FunkinMenu self, int curBeat)
        {
            if (self.SONG.Name != "joner") return;

            if (curBeat == 140)
            {
                StartCoroutine(Freeze(Conductor.crochet / 1000, self));
            }
        }

        private void FunkinMenu_OnPlayerHit(FunkinMenu self, RWF.Swagshit.Note daNote)
        {

            if (self.SONG.Name != "joner") return;

            /*  
             if (chars[note.noteType] != null) {
		        var char = chars[note.noteType];
		        var anim = ["singLEFT", "singDOWN", "singUP", "singRIGHT"][note.noteData % 4];
		        char.playAnim(anim, true);
		        char.holdTimer = 0;
		
		        if (char == jank) {
			        walkin = false;
			
			        jank.flipX = false;
			
			        if (anim == "singRIGHT") {
				        jank.velocity.x = 500;
			        } else {
				        jank.velocity.x = 0;
			        }
		        }
	        }*/

            if (chars.ContainsKey(daNote.noteType))
            {
                Character character = chars[daNote.noteType];

                switch (daNote.noteData)
                {
                    case 0:
                        character.PlayAnimation("left", true);
                        break;
                    case 1:
                        character.PlayAnimation("down", true);
                        break;
                    case 2:
                        character.PlayAnimation("up", true);
                        break;
                    case 3:
                        character.PlayAnimation("right", true);
                        break;
                }

                currentSingerTarget = character;

            }
            else
                currentSingerTarget = self.boyfriend;

        }

        private void FunkinMenu_OnCreate(FunkinMenu self)
        {

            chars = new Dictionary<string, Character>();

            if (self.SONG.Name != "joner") return;

            Character hunter = new(self, self.pages[0], File.ReadAllText(AssetManager.ResolveFilePath("funkin/characters/hunter_joner.json")));
            Character fivepebblesdiscordcall = new(self, self.pages[0], File.ReadAllText(AssetManager.ResolveFilePath("funkin/characters/fivepebblesdiscordcall_joner.json")));
            Character thatslutbitch = new(self, self.pages[0], File.ReadAllText(AssetManager.ResolveFilePath("funkin/characters/invjoner.json")));
            Character rivulta = new(self, self.pages[0], File.ReadAllText(AssetManager.ResolveFilePath("funkin/characters/rivuletjoner.json")));
            hunter.isPlayer = true;
            fivepebblesdiscordcall.isPlayer = true;
            thatslutbitch.isPlayer = true;
            rivulta.isPlayer = true;

            self.currentRappers.Add("hunter", hunter);
            self.currentRappers.Add("fivepebblesdiscordcall", fivepebblesdiscordcall);
            self.currentRappers.Add("thatslutbitch", thatslutbitch);
            self.currentRappers.Add("rivulta", rivulta);

            self.pages[0].subObjects.Add(rivulta);
            self.pages[0].subObjects.Add(hunter);
            self.pages[0].subObjects.Add(fivepebblesdiscordcall);
            self.pages[0].subObjects.Add(thatslutbitch);

            rivulta.sprite.MoveInFrontOfOtherNode(self.boyfriend.sprite);
            hunter.sprite.MoveInFrontOfOtherNode(hunter.sprite);
            fivepebblesdiscordcall.sprite.MoveBehindOtherNode(self.boyfriend.sprite);
            thatslutbitch.sprite.MoveBehindOtherNode(fivepebblesdiscordcall.sprite);

            rivulta.pos = hunter.pos = fivepebblesdiscordcall.pos = thatslutbitch.pos = self.boyfriend.pos;

            rivulta.pos = new Vector2(1185, 395);
            hunter.pos = new Vector2(1180, 410);
            hunter.pos += new Vector2(300, -80);
            thatslutbitch.pos += new Vector2(-315f, 0);
            thatslutbitch.pos.y = 425;
            fivepebblesdiscordcall.pos += new Vector2(100, 400);

            chars = new Dictionary<string, Character>()
            {
                ["char_jank"] = thatslutbitch,
                ["char_zomb"] = hunter,
                ["char_RBLXCYC"] = fivepebblesdiscordcall,
                ["char_krollge"] = rivulta
            };

            foreach (RWF.Swagshit.Note note in self.unspawnNotes)
            {
                foreach (string name in chars.Keys.ToList())
                {
                    if (note.noteType == name)
                    {
                        note.no_animation = true;
                        note.noMissAnimation = true;
                    }
                }
            }

            currentSingerTarget = self.boyfriend;

            //fivepebblesdiscordcall.sprite.shader = self.manager.rainWorld.Shaders["Hologram"];

        }

        private void FunkinMenu_OnStepHit(FunkinMenu self, int curStep)
        {

            if (curStep == 1542 && self.SONG.Name == "joner")
            {

                var data = AssetManager.ResolveFilePath("funkin/characters/eviljoner.json");

                Character newDad = new(self, self.pages[0], File.ReadAllText(data));

                newDad.flipped = !newDad.flipped;
                newDad.sprite.scaleX *= -1;
                self.pages[0].subObjects.Add(newDad);

                newDad.pos = new(self.stage.dad_pos.x + newDad.offsetPosition.x, self.stage.dad_pos.y + newDad.offsetPosition.y); ;

                newDad.PlayAnimation("transform");
                newDad.sprite.MoveInFrontOfOtherNode(self.dad.sprite);
                self.dad.Destroy();
                self.dad = newDad;
                self.currentRappers["dad"] = newDad;

                Logger.LogInfo("yuh");

            }

        }

        // Load any resources, such as sprites or sounds
        private void LoadResources(RainWorld rainWorld)
        {
            Futile.atlasManager.LoadAtlas("funkin/images/stages/su_a53");

            Futile.atlasManager.LoadImage("funkin/images/fivepebbles_joner_overseer");

            //Futile.atlasManager.LogAllElementNames();
        }

        IEnumerator Freeze(float sec, FunkinMenu funkinMenu)
        {

            FSprite glitch = new("pixel");
            glitch.color = Color.red;
            glitch.scale = 9999f;
            glitch.alpha = 0.5f;

            //glitch.shader = FShader.Additive;

            funkinMenu.pages[1].Container.AddChild(glitch);

            glitch.MoveToFront();

            Time.timeScale = 0;

            yield return new WaitForSecondsRealtime(sec);

            Time.timeScale = 1;

            funkinMenu.pages[1].Container.RemoveChild(glitch);

        }
        
    }
}