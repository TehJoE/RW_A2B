﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;
using RimWorld;

namespace A2B
{
    public class BeltLoaderComponent : BeltComponent
    {

        private bool hasStorageSettings; 

        public override void PostExposeData()
        {
            base.PostExposeData();

            Scribe_Values.LookValue<bool>(ref hasStorageSettings, "hasStorageSettings");
        }

        public override void PostSpawnSetup()
        {
            base.PostSpawnSetup();

            ISlotGroupParent slotParent = parent as ISlotGroupParent;
            if (slotParent == null)
            {
                throw new InvalidOperationException("parent is not a SlotGroupParent!");
            }

            // we kinda want to not overwrite custom storage settings every save/load...
            if (!hasStorageSettings)
				slotParent.GetStoreSettings().filter.SetDisallowAll();

            hasStorageSettings = true;
        }

        /**
         * Belt loaders don't freeze.
         **/
        protected override void Freeze()
        {
            // stub
        }

        /**
         * Belt loaders don't jam.
         **/
        public override void Jam()
        {
            // stub
        }

        /**
         * Belt loaders never accept items from other belt components.
         **/
        public override bool CanAcceptSomething()
        {
            return false;
        }

        protected override void PostItemContainerTick()
        {
            // Check the things that are on the ground at our position
            // If the thing can be moved and the destination is empty it can be added to our container
            // This should fix the "pawn carry items to the loader all the time"-bug
            foreach (var thing in Find.ThingGrid.ThingsAt(parent.Position))
            {
                if ((thing.def.category == ThingCategory.Item) && (thing != parent))
                {
                    var destination = GetDestinationForThing(thing);
                    var destBelt = destination.GetBeltComponent();

                    if (destBelt == null)
                    {
                        continue;
                    }

                    // Do not load items unless the next element can accept it
                    if (!destBelt.CanAcceptFrom(this))
                    {
                        continue;
                    }

					ItemContainer.AddItem(thing, BeltSpeed / 2);
                }
            }
        }
    }
}
