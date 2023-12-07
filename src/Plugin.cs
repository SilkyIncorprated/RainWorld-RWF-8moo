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
using System.Runtime.InteropServices;
using Rewired.UI.ControlMapper;

namespace EightMoo
{
    [BepInPlugin(MOD_ID, "RWF: 8moo", "1.1.0")]
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
        private FSprite blacker;

        private Character currentSingerTarget = null;

        //Import the following.
        [DllImport("user32.dll", EntryPoint = "SetWindowText")]
        public static extern bool SetWindowText(System.IntPtr hwnd, System.String lpString);

        [DllImport("user32.dll", EntryPoint = "FindWindow")]
        public static extern System.IntPtr FindWindow(System.String className, System.String windowName);

        private System.IntPtr CurrentWindow;

        private void ChangeWindowTitle(string title = "Rain World")
        {
            SetWindowText(CurrentWindow, title);
        }

        // Add hooks
        public void OnEnable()
        {
            On.RainWorld.OnModsInit += Extras.WrapInit(LoadResources);

            FunkinMenu.OnStepHit += FunkinMenu_OnStepHit;
            FunkinMenu.OnCreate += FunkinMenu_OnCreate;
            FunkinMenu.OnPlayerHit += FunkinMenu_OnPlayerHit;
            FunkinMenu.OnBeatHit += FunkinMenu_OnBeatHit;
            FunkinMenu.UpdateCameraTarget += FunkinMenu_UpdateCameraTarget;
            FunkinMenu.OnUpdate += FunkinMenu_OnUpdate;

            CurrentWindow = FindWindow(null, "Rain World");

        }

        private void FunkinMenu_OnUpdate(FunkinMenu self)
        {
            /*
             if getProperty('blacker.alpha') < 1 then
                if curBeat < 488 then
                    setProperty('blacker.alpha',getRandomInt(0,6)/100)
                elseif curBeat >= 488 and curBeat < 512 then 
                    setProperty('blacker.alpha',getRandomInt(0,3)/10)
                elseif curBeat >= 512 and curBeat < 524 then 
                    setProperty('blacker.alpha',getRandomInt(0,3)/10)
                elseif curBeat >= 524 and curBeat < 533 then 
                    setProperty('blacker.alpha',getRandomInt(0,8)/10)
                end
            end
            if luaSpriteExists('blackerr') then
                setProperty('blackerr.alpha',getRandomInt(0,3)/10)
            end
            */

            if (self.SONG.Name == "OGG")
            {
                var randomX = self.manager.rainWorld.screenSize.x * 3.1f;
                OGG_Bubble bubble = new(self, self.pages[0]);

                self.pages[0].subObjects.Add(bubble);

                bubble.pos.y = self.boyfriend.pos.y - 2300;
                bubble.pos.x += UnityEngine.Random.Range(-randomX / 1.5f, randomX);
                bubble.lastpos = bubble.pos;

                self.health = 2;
                
                if (Conductor.songPosition >= 177675 && self.dad.sprite.isVisible && self.stage.sprites[0].isVisible)
                {
                    //Application.Quit(); //this was a placeholder lol but i know what to do now

                    self.dad.sprite.isVisible = self.stage.sprites[0].isVisible = false;

                    ChangeWindowTitle();
                }

                if (Conductor.songPosition >= 180000 && !self.alreadygoingtoadifferentsecene)
                {
                    self.alreadygoingtoadifferentsecene = true;

                    self.manager.RequestMainProcessSwitch(ProcessManager.ProcessID.MainMenu, 12.5f);
                }

                if (blacker is not null)
                {
                    var curBeat = Conductor.curBeat;

                    if (curBeat == 64)
                    {
                        blacker.alpha = 0;
                    }

                    if (curBeat > 533 && blacker.alpha > 0)
                    {
                        var speed = 1f / 60f;
                        speed /= 10;

                        blacker.alpha -= speed;
                    }

                    if (blacker.alpha < 1)
                    {
                        if (curBeat == 533)
                        {
                            blacker.alpha = 1;
                        }
                        else if (curBeat < 488)
                        {
                            blacker.alpha = (float)UnityEngine.Random.Range(0, 6) / 100f;
                        }
                        else if (curBeat >= 488 && curBeat < 512)
                        {
                            blacker.alpha = (float)UnityEngine.Random.Range(0, 3) / 10f;
                        }
                        else if (curBeat >= 512 && curBeat < 524)
                        {
                            blacker.alpha = (float)UnityEngine.Random.Range(0, 3) / 10f;
                        }
                        else if (curBeat >= 524 && curBeat < 533)
                        {
                            blacker.alpha = (float)UnityEngine.Random.Range(0, 8) / 10f;            
                        }
                    }

                }

            }
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
            if (self.SONG.Name == "OGG" && curBeat == 4)
            {
                self.dad.sprite.MoveBehindOtherNode(self.boyfriend.sprite);

                if (curBeat == 380)
                    blacker.alpha = 1;
                else if (curBeat == 384)
                    blacker.alpha = 0;

            }

            if (self.SONG.Name == "joner")
            {
                if (curBeat == 140)
                {
                    StartCoroutine(Freeze(Conductor.crochet / 1000, self));
                }
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

            blacker = null;
            chars = new Dictionary<string, Character>();

            if (self.SONG.Name == "OGG")
            {

                ChangeWindowTitle("");

                self.useDefaultExitFunction = false;

                self.boyfriend.sprite.shader = self.dad.sprite.shader = self.stage.sprites[0].shader = self.manager.rainWorld.Shaders["Basic"];

                self.skipCountdown = true;

                self.bar.sprite.isVisible = self.hpIconP1.sprite.isVisible = self.hpIconP2.sprite.isVisible = self.scoretText.label.isVisible = false;
                foreach (StrumNote strum in self.opponentStrums)
                {
                    strum.sprite.isVisible = false;
                }

                foreach (RWF.Swagshit.Note note in self.unspawnNotes)
                {
                    if (!note.mustPress) note.sprite.isVisible = false;
                }

                self.dad.sprite.MoveBehindOtherNode(self.boyfriend.sprite);

                blacker = new("Futile_White");
                blacker.color = Color.black;
                blacker.scale = 9999;
                blacker.alpha = 1;

                self.pages[1].Container.AddChild(blacker);

                blacker.MoveToFront();

            }

            if (self.SONG.Name == "joner")
            {

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

                newDad.pos.y += (268 - 168);

                Logger.LogInfo("yuh");

            }

        }

        // Load any resources, such as sprites or sounds
        private void LoadResources(RainWorld rainWorld)
        {
            Futile.atlasManager.LoadAtlas("funkin/images/stages/su_a53");

            Futile.atlasManager.LoadImage("funkin/images/fivepebbles_joner_overseer");
            Futile.atlasManager.LoadImage("funkin/images/thelight");
            Futile.atlasManager.LoadImage("funkin/images/bubble");

            //Futile.atlasManager.LogAllElementNames();
        }

        IEnumerator Freeze(float sec, FunkinMenu funkinMenu)
        {

            FSprite glitch = new("pixel");
            glitch.color = Color.red;
            glitch.scale = 9999f;
            glitch.alpha = 0.5f;

            //glitch.shader = FShader.Additive;

            ChangeWindowTitle("Rain World (Not Responding)");

            funkinMenu.pages[1].Container.AddChild(glitch);

            glitch.MoveToFront();

            Time.timeScale = 0;

            yield return new WaitForSecondsRealtime(sec);

            Time.timeScale = 1;

            funkinMenu.pages[1].Container.RemoveChild(glitch);

            ChangeWindowTitle();

        }
        
    }
}