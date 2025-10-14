using UnityEngine;

namespace RimElShrine.Apparels.ApparelShield
{
    public class ApparelShieldInfo
    {
        public ApparelShieldInfo() { }
        public float overrideMaxEnergy = -1;
        public float overrideEnergyGainPerSecond = -1;
        public int overrideTicksToReset = -1;
        public bool showApparelDescription = true;
        public string? overrideLabel = null;
        public string? overrideDescription = null;
        public Color drawingColor = LocalColorOf.Default;
        //absorb emp damage or directly break.
        public bool absorbEMP = false;
        public float absorbEMPFactor = 1f;
        //absorb fire damage or ignore.
        public bool absorbEnviromental = false;
        public float absorbEnviromentalFactor = 1f;
        public bool absorbMelee = false;
        public float absorbMeleeFactor = 1f;
        public bool useBloodTransfer = false;
        public float bloodTransferMount = 0.1f;
        public float energyPerBlood = 10f;
    }
}
