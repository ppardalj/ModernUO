using System;
using System.Collections;
using Server.Engines.Plants;
using Server.Engines.Quests;
using Server.Engines.Quests.Hag;
using Server.Engines.Quests.Matriarch;
using Server.Mobiles;
using Server.Network;
using Server.Targeting;

namespace Server.Items
{
	public enum BeverageType
	{
		Ale,
		Cider,
		Liquor,
		Milk,
		Wine,
		Water
	}

	public interface IHasQuantity
	{
		int Quantity { get; set; }
	}

	public interface IWaterSource : IHasQuantity
	{
	}

	// TODO: Flippable attributes

	[TypeAlias( "Server.Items.BottleAle", "Server.Items.BottleLiquor", "Server.Items.BottleWine" )]
	public class BeverageBottle : BaseBeverage
	{
		public override int BaseLabelNumber => 1042959; // a bottle of Ale
		public override int MaxQuantity => 5;
		public override bool Fillable => false;

		public override int ComputeItemID()
		{
			if ( !IsEmpty )
			{
				switch( Content )
				{
					case BeverageType.Ale: return 0x99F;
					case BeverageType.Cider: return 0x99F;
					case BeverageType.Liquor: return 0x99B;
					case BeverageType.Milk: return 0x99B;
					case BeverageType.Wine: return 0x9C7;
					case BeverageType.Water: return 0x99B;
				}
			}

			return 0;
		}

		[Constructible]
		public BeverageBottle( BeverageType type )
			: base( type )
		{
			Weight = 1.0;
		}

		public BeverageBottle( Serial serial )
			: base( serial )
		{
		}

		public override void Serialize( GenericWriter writer )
		{
			base.Serialize( writer );

			writer.Write( (int)1 ); // version
		}

		public override void Deserialize( GenericReader reader )
		{
			base.Deserialize( reader );

			int version = reader.ReadInt();

			switch( version )
			{
				case 0:
					{
						if ( CheckType( "BottleAle" ) )
						{
							Quantity = MaxQuantity;
							Content = BeverageType.Ale;
						}
						else if ( CheckType( "BottleLiquor" ) )
						{
							Quantity = MaxQuantity;
							Content = BeverageType.Liquor;
						}
						else if ( CheckType( "BottleWine" ) )
						{
							Quantity = MaxQuantity;
							Content = BeverageType.Wine;
						}
						else
						{
							throw new Exception( World.LoadingType );
						}

						break;
					}
			}
		}
	}

	public class Jug : BaseBeverage
	{
		public override int BaseLabelNumber => 1042965; // a jug of Ale
		public override int MaxQuantity => 10;
		public override bool Fillable => false;

		public override int ComputeItemID()
		{
			if ( !IsEmpty )
				return 0x9C8;

			return 0;
		}

		[Constructible]
		public Jug( BeverageType type )
			: base( type )
		{
			Weight = 1.0;
		}

		public Jug( Serial serial )
			: base( serial )
		{
		}

		public override void Serialize( GenericWriter writer )
		{
			base.Serialize( writer );

			writer.Write( (int)1 ); // version
		}

		public override void Deserialize( GenericReader reader )
		{
			base.Deserialize( reader );

			int version = reader.ReadInt();
		}
	}

	public class CeramicMug : BaseBeverage
	{
		public override int BaseLabelNumber => 1042982; // a ceramic mug of Ale
		public override int MaxQuantity => 1;

		public override int ComputeItemID()
		{
			if ( ItemID >= 0x995 && ItemID <= 0x999 )
				return ItemID;
			if ( ItemID == 0x9CA )
				return ItemID;

			return 0x995;
		}

		[Constructible]
		public CeramicMug()
		{
			Weight = 1.0;
		}

		[Constructible]
		public CeramicMug( BeverageType type )
			: base( type )
		{
			Weight = 1.0;
		}

		public CeramicMug( Serial serial )
			: base( serial )
		{
		}

		public override void Serialize( GenericWriter writer )
		{
			base.Serialize( writer );

			writer.Write( (int)1 ); // version
		}

		public override void Deserialize( GenericReader reader )
		{
			base.Deserialize( reader );

			int version = reader.ReadInt();
		}
	}

	public class PewterMug : BaseBeverage
	{
		public override int BaseLabelNumber => 1042994; // a pewter mug with Ale
		public override int MaxQuantity => 1;

		public override int ComputeItemID()
		{
			if ( ItemID >= 0xFFF && ItemID <= 0x1002 )
				return ItemID;

			return 0xFFF;
		}

		[Constructible]
		public PewterMug()
		{
			Weight = 1.0;
		}

		[Constructible]
		public PewterMug( BeverageType type )
			: base( type )
		{
			Weight = 1.0;
		}

		public PewterMug( Serial serial )
			: base( serial )
		{
		}

		public override void Serialize( GenericWriter writer )
		{
			base.Serialize( writer );

			writer.Write( (int)1 ); // version
		}

		public override void Deserialize( GenericReader reader )
		{
			base.Deserialize( reader );

			int version = reader.ReadInt();
		}
	}

	public class Goblet : BaseBeverage
	{
		public override int BaseLabelNumber => 1043000; // a goblet of Ale
		public override int MaxQuantity => 1;

		public override int ComputeItemID()
		{
			if ( ItemID == 0x99A || ItemID == 0x9B3 || ItemID == 0x9BF || ItemID == 0x9CB )
				return ItemID;

			return 0x99A;
		}

		[Constructible]
		public Goblet()
		{
			Weight = 1.0;
		}

		[Constructible]
		public Goblet( BeverageType type )
			: base( type )
		{
			Weight = 1.0;
		}

		public Goblet( Serial serial )
			: base( serial )
		{
		}

		public override void Serialize( GenericWriter writer )
		{
			base.Serialize( writer );

			writer.Write( (int)1 ); // version
		}

		public override void Deserialize( GenericReader reader )
		{
			base.Deserialize( reader );

			int version = reader.ReadInt();
		}
	}

	[TypeAlias( "Server.Items.MugAle", "Server.Items.GlassCider", "Server.Items.GlassLiquor",
		 "Server.Items.GlassMilk", "Server.Items.GlassWine", "Server.Items.GlassWater" )]
	public class GlassMug : BaseBeverage
	{
		public override int EmptyLabelNumber => 1022456; // mug
		public override int BaseLabelNumber => 1042976; // a mug of Ale
		public override int MaxQuantity => 5;

		public override int ComputeItemID()
		{
			if ( IsEmpty )
				return ( ItemID >= 0x1F81 && ItemID <= 0x1F84 ? ItemID : 0x1F81 );

			switch( Content )
			{
				case BeverageType.Ale: return ( ItemID == 0x9EF ? 0x9EF : 0x9EE );
				case BeverageType.Cider: return ( ItemID >= 0x1F7D && ItemID <= 0x1F80 ? ItemID : 0x1F7D );
				case BeverageType.Liquor: return ( ItemID >= 0x1F85 && ItemID <= 0x1F88 ? ItemID : 0x1F85 );
				case BeverageType.Milk: return ( ItemID >= 0x1F89 && ItemID <= 0x1F8C ? ItemID : 0x1F89 );
				case BeverageType.Wine: return ( ItemID >= 0x1F8D && ItemID <= 0x1F90 ? ItemID : 0x1F8D );
				case BeverageType.Water: return ( ItemID >= 0x1F91 && ItemID <= 0x1F94 ? ItemID : 0x1F91 );
			}

			return 0;
		}

		[Constructible]
		public GlassMug()
		{
			Weight = 1.0;
		}

		[Constructible]
		public GlassMug( BeverageType type )
			: base( type )
		{
			Weight = 1.0;
		}

		public GlassMug( Serial serial )
			: base( serial )
		{
		}

		public override void Serialize( GenericWriter writer )
		{
			base.Serialize( writer );

			writer.Write( (int)1 ); // version
		}

		public override void Deserialize( GenericReader reader )
		{
			base.Deserialize( reader );

			int version = reader.ReadInt();

			switch( version )
			{
				case 0:
					{
						if ( CheckType( "MugAle" ) )
						{
							Quantity = MaxQuantity;
							Content = BeverageType.Ale;
						}
						else if ( CheckType( "GlassCider" ) )
						{
							Quantity = MaxQuantity;
							Content = BeverageType.Cider;
						}
						else if ( CheckType( "GlassLiquor" ) )
						{
							Quantity = MaxQuantity;
							Content = BeverageType.Liquor;
						}
						else if ( CheckType( "GlassMilk" ) )
						{
							Quantity = MaxQuantity;
							Content = BeverageType.Milk;
						}
						else if ( CheckType( "GlassWine" ) )
						{
							Quantity = MaxQuantity;
							Content = BeverageType.Wine;
						}
						else if ( CheckType( "GlassWater" ) )
						{
							Quantity = MaxQuantity;
							Content = BeverageType.Water;
						}
						else
						{
							throw new Exception( World.LoadingType );
						}

						break;
					}
			}
		}
	}

	[TypeAlias( "Server.Items.PitcherAle", "Server.Items.PitcherCider", "Server.Items.PitcherLiquor",
		"Server.Items.PitcherMilk", "Server.Items.PitcherWine", "Server.Items.PitcherWater",
		"Server.Items.GlassPitcher" )]
	public class Pitcher : BaseBeverage
	{
		public override int BaseLabelNumber => 1048128; // a Pitcher of Ale
		public override int MaxQuantity => 5;

		public override int ComputeItemID()
		{
			if ( IsEmpty )
			{
				if ( ItemID == 0x9A7 || ItemID == 0xFF7 )
					return ItemID;

				return 0xFF6;
			}

			switch( Content )
			{
				case BeverageType.Ale:
					{
						if ( ItemID == 0x1F96 )
							return ItemID;

						return 0x1F95;
					}
				case BeverageType.Cider:
					{
						if ( ItemID == 0x1F98 )
							return ItemID;

						return 0x1F97;
					}
				case BeverageType.Liquor:
					{
						if ( ItemID == 0x1F9A )
							return ItemID;

						return 0x1F99;
					}
				case BeverageType.Milk:
					{
						if ( ItemID == 0x9AD )
							return ItemID;

						return 0x9F0;
					}
				case BeverageType.Wine:
					{
						if ( ItemID == 0x1F9C )
							return ItemID;

						return 0x1F9B;
					}
				case BeverageType.Water:
					{
						if ( ItemID == 0xFF8 || ItemID == 0xFF9 || ItemID == 0x1F9E )
							return ItemID;

						return 0x1F9D;
					}
			}

			return 0;
		}

		[Constructible]
		public Pitcher()
		{
			Weight = 2.0;
		}

		[Constructible]
		public Pitcher( BeverageType type )
			: base( type )
		{
			Weight = 2.0;
		}

		public Pitcher( Serial serial )
			: base( serial )
		{
		}

		public override void Serialize( GenericWriter writer )
		{
			base.Serialize( writer );

			writer.Write( (int)1 ); // version
		}

		public override void Deserialize( GenericReader reader )
		{
			if ( CheckType( "PitcherWater" ) || CheckType( "GlassPitcher" ) )
				InternalDeserialize( reader, false );
			else
				InternalDeserialize( reader, true );

			int version = reader.ReadInt();

			switch( version )
			{
				case 0:
					{
						if ( CheckType( "PitcherAle" ) )
						{
							Quantity = MaxQuantity;
							Content = BeverageType.Ale;
						}
						else if ( CheckType( "PitcherCider" ) )
						{
							Quantity = MaxQuantity;
							Content = BeverageType.Cider;
						}
						else if ( CheckType( "PitcherLiquor" ) )
						{
							Quantity = MaxQuantity;
							Content = BeverageType.Liquor;
						}
						else if ( CheckType( "PitcherMilk" ) )
						{
							Quantity = MaxQuantity;
							Content = BeverageType.Milk;
						}
						else if ( CheckType( "PitcherWine" ) )
						{
							Quantity = MaxQuantity;
							Content = BeverageType.Wine;
						}
						else if ( CheckType( "PitcherWater" ) )
						{
							Quantity = MaxQuantity;
							Content = BeverageType.Water;
						}
						else if ( CheckType( "GlassPitcher" ) )
						{
							Quantity = 0;
							Content = BeverageType.Water;
						}
						else
						{
							throw new Exception( World.LoadingType );
						}

						break;
					}
			}
		}
	}

	public abstract class BaseBeverage : Item, IHasQuantity
	{
		private BeverageType m_Content;
		private int m_Quantity;

		public override int LabelNumber
		{
			get
			{
				int num = BaseLabelNumber;

				if ( IsEmpty || num == 0 )
					return EmptyLabelNumber;

				return BaseLabelNumber + (int)m_Content;
			}
		}

		public virtual bool ShowQuantity => ( MaxQuantity > 1 );
		public virtual bool Fillable  => true;
		public virtual bool Pourable  => true;

		public virtual int EmptyLabelNumber  => base.LabelNumber;
		public virtual int BaseLabelNumber  => 0;

		public abstract int MaxQuantity { get; }

		public abstract int ComputeItemID();

		[CommandProperty( AccessLevel.GameMaster )]
		public bool IsEmpty => ( m_Quantity <= 0 );

		[CommandProperty( AccessLevel.GameMaster )]
		public bool ContainsAlchohol => ( !IsEmpty && m_Content != BeverageType.Milk && m_Content != BeverageType.Water );

		[CommandProperty( AccessLevel.GameMaster )]
		public bool IsFull => ( m_Quantity >= MaxQuantity );

		[CommandProperty( AccessLevel.GameMaster )]
		public Poison Poison { get; set; }

		[CommandProperty( AccessLevel.GameMaster )]
		public Mobile Poisoner { get; set; }

		[CommandProperty( AccessLevel.GameMaster )]
		public BeverageType Content
		{
			get => m_Content;
			set
			{
				m_Content = value;

				InvalidateProperties();

				int itemID = ComputeItemID();

				if ( itemID > 0 )
					ItemID = itemID;
				else
					Delete();
			}
		}

		[CommandProperty( AccessLevel.GameMaster )]
		public int Quantity
		{
			get => m_Quantity;
			set
			{
				if ( value < 0 )
					value = 0;
				else if ( value > MaxQuantity )
					value = MaxQuantity;

				m_Quantity = value;

				InvalidateProperties();

				int itemID = ComputeItemID();

				if ( itemID > 0 )
					ItemID = itemID;
				else
					Delete();
			}
		}

		public virtual int GetQuantityDescription()
		{
			int perc = ( m_Quantity * 100 ) / MaxQuantity;

			if ( perc <= 0 )
				return 1042975; // It's empty.
			if ( perc <= 33 )
				return 1042974; // It's nearly empty.
			if ( perc <= 66 )
				return 1042973; // It's half full.
			return 1042972; // It's full.
		}

		public override void GetProperties( ObjectPropertyList list )
		{
			base.GetProperties( list );

			if ( ShowQuantity )
				list.Add( GetQuantityDescription() );
		}

		public override void OnSingleClick( Mobile from )
		{
			base.OnSingleClick( from );

			if ( ShowQuantity )
				LabelTo( from, GetQuantityDescription() );
		}

		public virtual bool ValidateUse( Mobile from, bool message )
		{
			if ( Deleted )
				return false;

			if ( !Movable && !Fillable )
			{
				Multis.BaseHouse house = Multis.BaseHouse.FindHouseAt( this );

				if ( house == null || !house.IsLockedDown( this ) )
				{
					if ( message )
						from.SendLocalizedMessage( 502946, "", 0x59 ); // That belongs to someone else.

					return false;
				}
			}

			if ( from.Map != Map || !from.InRange( GetWorldLocation(), 2 ) || !from.InLOS( this ) )
			{
				if ( message )
					from.LocalOverheadMessage( MessageType.Regular, 0x3B2, 1019045 ); // I can't reach that.

				return false;
			}

			return true;
		}

		public virtual void Fill_OnTarget( Mobile from, object targ )
		{
			if ( !IsEmpty || !Fillable || !ValidateUse( from, false ) )
				return;

			if ( targ is BaseBeverage bev )
			{
				if ( bev.IsEmpty || !bev.ValidateUse( from, true ) )
					return;

				Content = bev.Content;
				Poison = bev.Poison;
				Poisoner = bev.Poisoner;

				if ( bev.Quantity > MaxQuantity )
				{
					Quantity = MaxQuantity;
					bev.Quantity -= MaxQuantity;
				}
				else
				{
					Quantity += bev.Quantity;
					bev.Quantity = 0;
				}
			}
			else if ( targ is BaseWaterContainer bwc )
			{
				if ( Quantity == 0 || ( Content == BeverageType.Water && !IsFull ) )
				{
					int iNeed = Math.Min( ( MaxQuantity - Quantity ), bwc.Quantity );

					if ( iNeed > 0 && !bwc.IsEmpty && !IsFull )
					{
						bwc.Quantity -= iNeed;
						Quantity += iNeed;
						Content = BeverageType.Water;

						from.PlaySound( 0x4E );
					}
				}
			}
			else if ( targ is Item item )
			{
				IWaterSource src = ( item as IWaterSource );

				if ( src == null && item is AddonComponent component )
					src = ( component.Addon as IWaterSource );

				if ( src == null || src.Quantity <= 0 )
					return;

				if ( from.Map != item.Map || !from.InRange( item.GetWorldLocation(), 2 ) || !from.InLOS( item ) )
				{
					from.LocalOverheadMessage( MessageType.Regular, 0x3B2, 1019045 ); // I can't reach that.
					return;
				}

				Content = BeverageType.Water;
				Poison = null;
				Poisoner = null;

				if ( src.Quantity > MaxQuantity )
				{
					Quantity = MaxQuantity;
					src.Quantity -= MaxQuantity;
				}
				else
				{
					Quantity += src.Quantity;
					src.Quantity = 0;
				}

				from.SendLocalizedMessage( 1010089 ); // You fill the container with water.
			}
			else if ( targ is Cow cow )
			{
				if ( cow.TryMilk( from ) )
				{
					Content = BeverageType.Milk;
					Quantity = MaxQuantity;
					from.SendLocalizedMessage( 1080197 ); // You fill the container with milk.
				}
			}
			else if ( targ is LandTarget target )
			{
				int tileID = target.TileID;

				if ( from is PlayerMobile player )
				{
					QuestSystem qs = player.Quest;

					if ( qs is WitchApprenticeQuest )
					{
						if ( qs.FindObjective( typeof( FindIngredientObjective ) ) is FindIngredientObjective obj && !obj.Completed && obj.Ingredient == Ingredient.SwampWater )
						{
							bool contains = false;

							for( int i = 0; !contains && i < m_SwampTiles.Length; i += 2 )
								contains = ( tileID >= m_SwampTiles[ i ] && tileID <= m_SwampTiles[ i + 1 ] );

							if ( contains )
							{
								Delete();

								player.SendLocalizedMessage( 1055035 ); // You dip the container into the disgusting swamp water, collecting enough for the Hag's vile stew.
								obj.Complete();
							}
						}
					}
				}
			}
		}

		private static int[] m_SwampTiles = {
				0x9C4, 0x9EB,
				0x3D65, 0x3D65,
				0x3DC0, 0x3DD9,
				0x3DDB, 0x3DDC,
				0x3DDE, 0x3EF0,
				0x3FF6, 0x3FF6,
				0x3FFC, 0x3FFE,
			};

		#region Effects of achohol
		private static Hashtable m_Table = new Hashtable();

		public static void Initialize()
		{
			EventSink.Login += EventSink_Login;
		}

		private static void EventSink_Login( LoginEventArgs e )
		{
			CheckHeaveTimer( e.Mobile );
		}

		public static void CheckHeaveTimer( Mobile from )
		{
			if ( from.BAC > 0 && from.Map != Map.Internal && !from.Deleted )
			{
				Timer t = (Timer)m_Table[ from ];

				if ( t == null )
				{
					if ( from.BAC > 60 )
						from.BAC = 60;

					t = new HeaveTimer( from );
					t.Start();

					m_Table[ from ] = t;
				}
			}
			else
			{
				Timer t = (Timer)m_Table[ from ];

				if ( t != null )
				{
					t.Stop();
					m_Table.Remove( from );

					from.SendLocalizedMessage( 500850 ); // You feel sober.
				}
			}
		}

		private class HeaveTimer : Timer
		{
			private Mobile m_Drunk;

			public HeaveTimer( Mobile drunk )
				: base( TimeSpan.FromSeconds( 5.0 ), TimeSpan.FromSeconds( 5.0 ) )
			{
				m_Drunk = drunk;

				Priority = TimerPriority.OneSecond;
			}

			protected override void OnTick()
			{
				if ( m_Drunk.Deleted || m_Drunk.Map == Map.Internal )
				{
					Stop();
					m_Table.Remove( m_Drunk );
				}
				else if ( m_Drunk.Alive )
				{
					if ( m_Drunk.BAC > 60 )
						m_Drunk.BAC = 60;

					// chance to get sober
					if ( 10 > Utility.Random( 100 ) )
						--m_Drunk.BAC;

					// lose some stats
					m_Drunk.Stam -= 1;
					m_Drunk.Mana -= 1;

					if ( Utility.Random( 1, 4 ) == 1 )
					{
						if ( !m_Drunk.Mounted )
						{
							// turn in a random direction
							m_Drunk.Direction = (Direction)Utility.Random( 8 );

							// heave
							m_Drunk.Animate( 32, 5, 1, true, false, 0 );
						}

						// *hic*
						m_Drunk.PublicOverheadMessage( MessageType.Regular, 0x3B2, 500849 );
					}

					if ( m_Drunk.BAC <= 0 )
					{
						Stop();
						m_Table.Remove( m_Drunk );

						m_Drunk.SendLocalizedMessage( 500850 ); // You feel sober.
					}
				}
			}
		}

		#endregion

		public virtual void Pour_OnTarget( Mobile from, object targ )
		{
			if ( IsEmpty || !Pourable || !ValidateUse( from, false ) )
				return;

			if ( targ is BaseBeverage bev )
			{
				if ( !bev.ValidateUse( from, true ) )
					return;

				if ( bev.IsFull && bev.Content == Content )
				{
					from.SendLocalizedMessage( 500848 ); // Couldn't pour it there.  It was already full.
				}
				else if ( !bev.IsEmpty )
				{
					from.SendLocalizedMessage( 500846 ); // Can't pour it there.
				}
				else
				{
					bev.Content = Content;
					bev.Poison = Poison;
					bev.Poisoner = Poisoner;

					if ( Quantity > bev.MaxQuantity )
					{
						bev.Quantity = bev.MaxQuantity;
						Quantity -= bev.MaxQuantity;
					}
					else
					{
						bev.Quantity += Quantity;
						Quantity = 0;
					}

					from.PlaySound( 0x4E );
				}
			}
			else if ( from == targ )
			{
				if ( from.Thirst < 20 )
					from.Thirst += 1;

				if ( ContainsAlchohol )
				{
					int bac = 0;

					switch( Content )
					{
						case BeverageType.Ale: bac = 1; break;
						case BeverageType.Wine: bac = 2; break;
						case BeverageType.Cider: bac = 3; break;
						case BeverageType.Liquor: bac = 4; break;
					}

					from.BAC += bac;

					if ( from.BAC > 60 )
						from.BAC = 60;

					CheckHeaveTimer( from );
				}

				from.PlaySound( Utility.RandomList( 0x30, 0x2D6 ) );

				if ( Poison != null )
					from.ApplyPoison( Poisoner, Poison );

				--Quantity;
			}
			else if ( targ is BaseWaterContainer bwc )
			{
				if ( Content != BeverageType.Water )
				{
					from.SendLocalizedMessage( 500842 ); // Can't pour that in there.
				}
				else if ( bwc.Items.Count != 0 )
				{
					from.SendLocalizedMessage( 500841 ); // That has something in it.
				}
				else
				{
					int itNeeds = Math.Min( ( bwc.MaxQuantity - bwc.Quantity ), Quantity );

					if ( itNeeds > 0 )
					{
						bwc.Quantity += itNeeds;
						Quantity -= itNeeds;

						from.PlaySound( 0x4E );
					}
				}
			}
			else if ( targ is PlantItem item )
			{
				item.Pour( from, this );
			}
			else if ( targ is AddonComponent component &&
				( component.Addon is WaterVatEast || component.Addon is WaterVatSouth ) &&
				Content == BeverageType.Water )
			{
				if ( from is PlayerMobile player )
				{
					if ( player.Quest is SolenMatriarchQuest qs )
					{
						QuestObjective obj = qs.FindObjective( typeof( GatherWaterObjective ) );

						if ( obj != null && !obj.Completed )
						{
							BaseAddon vat = component.Addon;

							if ( vat.X > 5784 && vat.X < 5814 && vat.Y > 1903 && vat.Y < 1934 &&
								( ( qs.RedSolen && vat.Map == Map.Trammel ) || ( !qs.RedSolen && vat.Map == Map.Felucca ) ) )
							{
								if ( obj.CurProgress + Quantity > obj.MaxProgress )
								{
									int delta = obj.MaxProgress - obj.CurProgress;

									Quantity -= delta;
									obj.CurProgress = obj.MaxProgress;
								}
								else
								{
									obj.CurProgress += Quantity;
									Quantity = 0;
								}
							}
						}
					}
				}
			}
			else
			{
				from.SendLocalizedMessage( 500846 ); // Can't pour it there.
			}
		}

		public override void OnDoubleClick( Mobile from )
		{
			if ( IsEmpty )
			{
				if ( !Fillable || !ValidateUse( from, true ) )
					return;

				from.BeginTarget( -1, true, TargetFlags.None, Fill_OnTarget );
				SendLocalizedMessageTo( from, 500837 ); // Fill from what?
			}
			else if ( Pourable && ValidateUse( from, true ) )
			{
				from.BeginTarget( -1, true, TargetFlags.None, Pour_OnTarget );
				from.SendLocalizedMessage( 1010086 ); // What do you want to use this on?
			}
		}

		public static bool ConsumeTotal( Container pack, BeverageType content, int quantity )
		{
			return ConsumeTotal( pack, typeof( BaseBeverage ), content, quantity );
		}

		public static bool ConsumeTotal( Container pack, Type itemType, BeverageType content, int quantity )
		{
			Item[] items = pack.FindItemsByType( itemType );

			// First pass, compute total
			int total = 0;

			for( int i = 0; i < items.Length; ++i )
			{
				if ( items[i] is BaseBeverage bev && bev.Content == content && !bev.IsEmpty )
					total += bev.Quantity;
			}

			if ( total >= quantity )
			{
				// We've enough, so consume it

				int need = quantity;

				for( int i = 0; i < items.Length; ++i )
				{
					if ( !(items[i] is BaseBeverage bev) || bev.Content != content || bev.IsEmpty )
						continue;

					int theirQuantity = bev.Quantity;

					if ( theirQuantity < need )
					{
						bev.Quantity = 0;
						need -= theirQuantity;
					}
					else
					{
						bev.Quantity -= need;
						return true;
					}
				}
			}

			return false;
		}

		public BaseBeverage()
		{
			ItemID = ComputeItemID();
		}

		public BaseBeverage( BeverageType type )
		{
			m_Content = type;
			m_Quantity = MaxQuantity;
			ItemID = ComputeItemID();
		}

		public BaseBeverage( Serial serial )
			: base( serial )
		{
		}

		public override void Serialize( GenericWriter writer )
		{
			base.Serialize( writer );

			writer.Write( (int)1 ); // version

			writer.Write( (Mobile)Poisoner );

			Poison.Serialize( Poison, writer );
			writer.Write( (int)m_Content );
			writer.Write( (int)m_Quantity );
		}

		protected bool CheckType( string name )
		{
			return ( World.LoadingType == $"Server.Items.{name}");
		}

		public override void Deserialize( GenericReader reader )
		{
			InternalDeserialize( reader, true );
		}

		protected void InternalDeserialize( GenericReader reader, bool read )
		{
			base.Deserialize( reader );

			if ( !read )
				return;

			int version = reader.ReadInt();

			switch( version )
			{
				case 1:
					{
						Poisoner = reader.ReadMobile();
						goto case 0;
					}
				case 0:
					{
						Poison = Poison.Deserialize( reader );
						m_Content = (BeverageType)reader.ReadInt();
						m_Quantity = reader.ReadInt();
						break;
					}
			}
		}
	}
}
