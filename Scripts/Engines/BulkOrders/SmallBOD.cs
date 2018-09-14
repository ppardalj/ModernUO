using System;
using Server.Items;
using System.Collections.Generic;
using Server.Mobiles;

namespace Server.Engines.BulkOrders
{
	[TypeAlias( "Scripts.Engines.BulkOrders.SmallBOD" )]
	public abstract class SmallBOD : Item
	{
		private int m_AmountCur, m_AmountMax;
		private int m_Number;
		private bool m_RequireExceptional;
		private BulkMaterialType m_Material;

		[CommandProperty( AccessLevel.GameMaster )]
		public int AmountCur{ get => m_AmountCur;
			set{ m_AmountCur = value; InvalidateProperties(); } }

		[CommandProperty( AccessLevel.GameMaster )]
		public int AmountMax{ get => m_AmountMax;
			set{ m_AmountMax = value; InvalidateProperties(); } }

		[CommandProperty( AccessLevel.GameMaster )]
		public Type Type { get; set; }

		[CommandProperty( AccessLevel.GameMaster )]
		public int Number{ get => m_Number;
			set{ m_Number = value; InvalidateProperties(); } }

		[CommandProperty( AccessLevel.GameMaster )]
		public int Graphic { get; set; }

		[CommandProperty( AccessLevel.GameMaster )]
		public bool RequireExceptional{ get => m_RequireExceptional;
			set{ m_RequireExceptional = value; InvalidateProperties(); } }

		[CommandProperty( AccessLevel.GameMaster )]
		public BulkMaterialType Material{ get => m_Material;
			set{ m_Material = value; InvalidateProperties(); } }

		[CommandProperty( AccessLevel.GameMaster )]
		public bool Complete => ( m_AmountCur == m_AmountMax );

		public override int LabelNumber => 1045151; // a bulk order deed

		[Constructible]
		public SmallBOD( int hue, int amountMax, Type type, int number, int graphic, bool requireExeptional, BulkMaterialType material ) : base( Core.AOS ? 0x2258 : 0x14EF )
		{
			Weight = 1.0;
			Hue = hue; // Blacksmith: 0x44E; Tailoring: 0x483
			LootType = LootType.Blessed;

			m_AmountMax = amountMax;
			Type = type;
			m_Number = number;
			Graphic = graphic;
			m_RequireExceptional = requireExeptional;
			m_Material = material;
		}

		public SmallBOD() : base( Core.AOS ? 0x2258 : 0x14EF )
		{
			Weight = 1.0;
			LootType = LootType.Blessed;
		}

		public static BulkMaterialType GetRandomMaterial( BulkMaterialType start, double[] chances )
		{
			double random = Utility.RandomDouble();

			for ( int i = 0; i < chances.Length; ++i )
			{
				if ( random < chances[i] )
					return ( i == 0 ? BulkMaterialType.None : start + (i - 1) );

				random -= chances[i];
			}

			return BulkMaterialType.None;
		}

		public override void GetProperties( ObjectPropertyList list )
		{
			base.GetProperties( list );

			list.Add( 1060654 ); // small bulk order

			if ( m_RequireExceptional )
				list.Add( 1045141 ); // All items must be exceptional.

			if ( m_Material != BulkMaterialType.None )
				list.Add( SmallBODGump.GetMaterialNumberFor( m_Material ) ); // All items must be made with x material.

			list.Add( 1060656, m_AmountMax.ToString() ); // amount to make: ~1_val~
			list.Add( 1060658, "#{0}\t{1}", m_Number, m_AmountCur ); // ~1_val~: ~2_val~
		}

		public override void OnDoubleClick( Mobile from )
		{
			if ( IsChildOf( from.Backpack ) || InSecureTrade || RootParent is PlayerVendor )
				from.SendGump( new SmallBODGump( from, this ) );
			else
				from.SendLocalizedMessage( 1045156 ); // You must have the deed in your backpack to use it.
		}

		public override void OnDoubleClickNotAccessible( Mobile from )
		{
			OnDoubleClick( from );
		}

		public override void OnDoubleClickSecureTrade( Mobile from )
		{
			OnDoubleClick( from );
		}

		public void BeginCombine( Mobile from )
		{
			if ( m_AmountCur < m_AmountMax )
				from.Target = new SmallBODTarget( this );
			else
				from.SendLocalizedMessage( 1045166 ); // The maximum amount of requested items have already been combined to this deed.
		}

		public abstract List<Item> ComputeRewards( bool full );
		public abstract int ComputeGold();
		public abstract int ComputeFame();

		public virtual void GetRewards( out Item reward, out int gold, out int fame )
		{
			reward = null;
			gold = ComputeGold();
			fame = ComputeFame();

			List<Item> rewards = ComputeRewards( false );

			if ( rewards.Count > 0 )
			{
				reward = rewards[Utility.Random( rewards.Count )];

				for ( int i = 0; i < rewards.Count; ++i )
				{
					if ( rewards[i] != reward )
						rewards[i].Delete();
				}
			}
		}

		public static BulkMaterialType GetMaterial( CraftResource resource )
		{
			switch ( resource )
			{
				case CraftResource.DullCopper:		return BulkMaterialType.DullCopper;
				case CraftResource.ShadowIron:		return BulkMaterialType.ShadowIron;
				case CraftResource.Copper:			return BulkMaterialType.Copper;
				case CraftResource.Bronze:			return BulkMaterialType.Bronze;
				case CraftResource.Gold:			return BulkMaterialType.Gold;
				case CraftResource.Agapite:			return BulkMaterialType.Agapite;
				case CraftResource.Verite:			return BulkMaterialType.Verite;
				case CraftResource.Valorite:		return BulkMaterialType.Valorite;
				case CraftResource.SpinedLeather:	return BulkMaterialType.Spined;
				case CraftResource.HornedLeather:	return BulkMaterialType.Horned;
				case CraftResource.BarbedLeather:	return BulkMaterialType.Barbed;
			}

			return BulkMaterialType.None;
		}

		public void EndCombine( Mobile from, object o )
		{
			if ( o is Item item && item.IsChildOf( from.Backpack ) )
			{
				Type objectType = item.GetType();

				if ( m_AmountCur >= m_AmountMax )
				{
					from.SendLocalizedMessage( 1045166 ); // The maximum amount of requested items have already been combined to this deed.
				}
				else if ( Type == null || (objectType != Type && !objectType.IsSubclassOf( Type )) || (!(item is BaseWeapon) && !(item is BaseArmor) && !(item is BaseClothing)) )
				{
					from.SendLocalizedMessage( 1045169 ); // The item is not in the request.
				}
				else
				{
					BaseArmor armor = item as BaseArmor;
					BaseClothing clothing = item as BaseClothing;

					BulkMaterialType material = GetMaterial( armor?.Resource ?? clothing?.Resource ?? CraftResource.None );

					if ( m_Material >= BulkMaterialType.DullCopper && m_Material <= BulkMaterialType.Valorite && material != m_Material )
					{
						from.SendLocalizedMessage( 1045168 ); // The item is not made from the requested ore.
					}
					else if ( m_Material >= BulkMaterialType.Spined && m_Material <= BulkMaterialType.Barbed && material != m_Material )
					{
						from.SendLocalizedMessage( 1049352 ); // The item is not made from the requested leather type.
					}
					else
					{
						bool isExceptional;

						if ( item is BaseWeapon weapon )
							isExceptional = weapon.Quality == WeaponQuality.Exceptional;
						else if ( armor != null )
							isExceptional = armor.Quality == ArmorQuality.Exceptional;
						else
							isExceptional = clothing.Quality == ClothingQuality.Exceptional;

						if ( m_RequireExceptional && !isExceptional )
						{
							from.SendLocalizedMessage( 1045167 ); // The item must be exceptional.
						}
						else
						{
							item.Delete();
							++AmountCur;

							from.SendLocalizedMessage( 1045170 ); // The item has been combined with the deed.

							from.SendGump( new SmallBODGump( from, this ) );

							if ( m_AmountCur < m_AmountMax )
								BeginCombine( from );
						}
					}
				}
			}
			else
			{
				from.SendLocalizedMessage( 1045158 ); // You must have the item in your backpack to target it.
			}
		}

		public SmallBOD( Serial serial ) : base( serial )
		{
		}

		public override void Serialize( GenericWriter writer )
		{
			base.Serialize( writer );

			writer.Write( (int) 0 ); // version

			writer.Write( m_AmountCur );
			writer.Write( m_AmountMax );
			writer.Write( Type == null ? null : Type.FullName );
			writer.Write( m_Number );
			writer.Write( Graphic );
			writer.Write( m_RequireExceptional );
			writer.Write( (int) m_Material );
		}

		public override void Deserialize( GenericReader reader )
		{
			base.Deserialize( reader );

			int version = reader.ReadInt();

			switch ( version )
			{
				case 0:
				{
					m_AmountCur = reader.ReadInt();
					m_AmountMax = reader.ReadInt();

					string type = reader.ReadString();

					if ( type != null )
						Type = ScriptCompiler.FindTypeByFullName( type );

					m_Number = reader.ReadInt();
					Graphic = reader.ReadInt();
					m_RequireExceptional = reader.ReadBool();
					m_Material = (BulkMaterialType)reader.ReadInt();

					break;
				}
			}

			if ( Weight == 0.0 )
				Weight = 1.0;

			if ( Core.AOS && ItemID == 0x14EF )
				ItemID = 0x2258;

			if ( Parent == null && Map == Map.Internal && Location == Point3D.Zero )
				Delete();
		}
	}
}
