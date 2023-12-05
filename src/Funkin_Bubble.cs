using Menu;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace EightMoo
{
    internal class OGG_Bubble : RWF.Swagshit.FunkinSprite
    {

        public Vector2 acc = Vector2.zero;
        public Vector2 vel = Vector2.zero;
        public int lifetime = 0;

        public OGG_Bubble(Menu.Menu menu, MenuObject owner) : base(menu, owner)
        {
            this.sprite = new("funkin/images/bubble");

            this.Container.AddChild(sprite);

            this.sprite.MoveToFront();

            sprite.color = Color.black;
            sprite.alpha = UnityEngine.Random.Range(0.1f, 0.5f);
            sprite.scale = UnityEngine.Random.Range(2.5f, 4f);
            scrollFactor = new(UnityEngine.Random.Range(0.5f, 3f), 2);
            sprite.SetAnchor(0.5f, 0.5f);

            acc = new(0, 1.5f);
            vel.x = UnityEngine.Random.Range(-30, -12.5f);
        }

        public override void Update()
        {
            base.Update();

            this.pos += vel;
            this.vel += acc;

            lifetime++;

            if (lifetime > 180)
                Destroy();
        }

    }

}
