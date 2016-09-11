
namespace ServerMod.cmds
{
    enum PermType
    {
        ALL,
        HOST,
        LOCAL
    };

    abstract class cmd
    {
        public abstract string name { get; }
        public abstract PermType perm { get; }
        public abstract bool canUseAsClient { get; }

        public abstract void help(ClientPlayerInfo p);
        public abstract void use(ClientPlayerInfo p, string message);

        public static cmdlist all = new cmdlist();
    }
}
