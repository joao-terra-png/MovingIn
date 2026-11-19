namespace MovingIn.Minigames.Laundry{
    public enum ClothColor { White, Dark, Colored }
    public enum ClothType { Shirt, Pants, Sock }
    public enum LaundryPhase { Wash, Washing, Store, Done }

    public static class ClothNames
    {
        public const int ColorCount = 3;
        public const int TypeCount = 3;

        public static string Of(ClothColor c) => c switch
        {
            ClothColor.White => "Brancas",
            ClothColor.Dark => "Escuras",
            _ => "Coloridas"
        };

        public static string Of(ClothType t) => t switch
        {
            ClothType.Shirt => "Camisetas",
            ClothType.Pants => "Calças",
            _ => "Meias"
        };
    }
}
