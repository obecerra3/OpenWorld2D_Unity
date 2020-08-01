namespace Structs
{
    [System.Serializable]
    public struct PlayerSave
    {
        public string name;
        public SerializableVector3 position;
        public int health;
        public int mana;
        public int max_health;
        public int max_mana;
    };

    [System.Serializable]
    public struct WorldSave {};
}
