namespace Server.Engines.Mahjong
{
	public class MahjongTile
	{
		public static MahjongPieceDim GetDimensions( Point2D position, MahjongPieceDirection direction )
		{
			if ( direction == MahjongPieceDirection.Up || direction == MahjongPieceDirection.Down )
				return new MahjongPieceDim( position, 20, 30 );
			return new MahjongPieceDim( position, 30, 20 );
		}

		protected Point2D m_Position;

		public MahjongGame Game { get; }

		public int Number { get; }

		public MahjongTileType Value { get; }

		public Point2D Position  => m_Position;
		public int StackLevel { get; private set; }

		public MahjongPieceDirection Direction { get; private set; }

		public bool Flipped { get; private set; }

		public MahjongTile( MahjongGame game, int number, MahjongTileType value, Point2D position, int stackLevel, MahjongPieceDirection direction, bool flipped )
		{
			Game = game;
			Number = number;
			Value = value;
			m_Position = position;
			StackLevel = stackLevel;
			Direction = direction;
			Flipped = flipped;
		}

		public MahjongPieceDim Dimensions => GetDimensions( m_Position, Direction );

		public bool IsMovable => Game.GetStackLevel( Dimensions ) <= StackLevel;

		public void Move( Point2D position, MahjongPieceDirection direction, bool flip, int validHandArea )
		{
			MahjongPieceDim dim = GetDimensions( position, direction );
			int curHandArea = Dimensions.GetHandArea();
			int newHandArea = dim.GetHandArea();

			if ( !IsMovable || !dim.IsValid() || ( validHandArea >= 0 && ((curHandArea >= 0 && curHandArea != validHandArea) || (newHandArea >= 0 && newHandArea != validHandArea)) ) )
				return;

			m_Position = position;
			Direction = direction;
			StackLevel = -1; // Avoid self interference
			StackLevel = Game.GetStackLevel( dim ) + 1;
			Flipped = flip;

			Game.Players.SendTilePacket( this, true, true );
		}

		public void Save( GenericWriter writer )
		{
			writer.Write( (int) 0 ); // version

			writer.Write( Number );
			writer.Write( (int) Value );
			writer.Write( m_Position );
			writer.Write( StackLevel );
			writer.Write( (int) Direction );
			writer.Write( Flipped );
		}

		public MahjongTile( MahjongGame game, GenericReader reader )
		{
			Game = game;

			int version = reader.ReadInt();

			Number = reader.ReadInt();
			Value = (MahjongTileType) reader.ReadInt();
			m_Position = reader.ReadPoint2D();
			StackLevel = reader.ReadInt();
			Direction = (MahjongPieceDirection) reader.ReadInt();
			Flipped = reader.ReadBool();
		}
	}
}
