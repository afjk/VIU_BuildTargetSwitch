namespace AFJK.BuildConfigSwitch
{
    public class BuildConfigSwitcher
    {
        readonly BuildConfigScriptableObject buildConfigScriptableObject;
        public BuildConfigSwitcher(BuildConfigScriptableObject buildConfigScriptableObject)
        {
            this.buildConfigScriptableObject = buildConfigScriptableObject;
        }
    }
}