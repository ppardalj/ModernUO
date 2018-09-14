﻿using System;
using System.Reflection;
using Server.Multis;

namespace Server.Items
{
	[AttributeUsage( AttributeTargets.Class )]
	public class FlippableAddonAttribute : Attribute
	{
		private static string m_MethodName = "Flip";

		private static Type[] m_Params = {
			typeof( Mobile ), typeof( Direction )
		};

		public Direction[] Directions { get; }

		public FlippableAddonAttribute( params Direction[] directions )
		{
			Directions = directions;
		}

		public virtual void Flip( Mobile from, Item addon )
		{
			if ( Directions != null && Directions.Length > 1 )
			{
				try
				{
					MethodInfo flipMethod = addon.GetType().GetMethod( m_MethodName, m_Params );

					if ( flipMethod != null )
					{
						int index = 0;

						for ( int i = 0; i < Directions.Length; i++ )
						{
							if ( addon.Direction == Directions[ i ] )
							{
								index = i + 1;
								break;
							}
						}

						if ( index >= Directions.Length )
							index = 0;

						ClearComponents( addon );

						flipMethod.Invoke( addon, new object[ 2 ] { from, Directions[ index ] } );

						BaseHouse house = null;
						AddonFitResult result = AddonFitResult.Valid;

						addon.Map = Map.Internal;

						if ( addon is BaseAddon baseAddon )
							result = baseAddon.CouldFit( baseAddon.Location, from.Map, from, ref house );
						else if ( addon is BaseAddonContainer container )
							result = container.CouldFit( container.Location, from.Map, from, ref house );

						addon.Map = from.Map;

						if ( result != AddonFitResult.Valid )
						{
							if ( index == 0 )
								index = Directions.Length - 1;
							else
								index -= 1;

							ClearComponents( addon );

							flipMethod.Invoke( addon, new object[ 2 ] { from, Directions[ index ] } );

							if ( result == AddonFitResult.Blocked )
								from.SendLocalizedMessage( 500269 ); // You cannot build that there.
							else if ( result == AddonFitResult.NotInHouse )
								from.SendLocalizedMessage( 500274 ); // You can only place this in a house that you own!
							else if ( result == AddonFitResult.DoorsNotClosed )
								from.SendMessage( "You must close all house doors before placing this." );
							else if ( result == AddonFitResult.DoorTooClose )
								from.SendLocalizedMessage( 500271 ); // You cannot build near the door.
							else if ( result == AddonFitResult.NoWall )
								from.SendLocalizedMessage( 500268 ); // This object needs to be mounted on something.
						}

						addon.Direction = Directions[ index ];
					}
				}
				catch
				{
				}
			}
		}

		private void ClearComponents( Item item )
		{
			if ( item is BaseAddon addon )
			{
				foreach ( AddonComponent c in addon.Components )
				{
					c.Addon = null;
					c.Delete();
				}

				addon.Components.Clear();
			}
			else if ( item is BaseAddonContainer addonContainer )
			{
				foreach ( AddonContainerComponent c in addonContainer.Components )
				{
					c.Addon = null;
					c.Delete();
				}

				addonContainer.Components.Clear();
			}
		}
	}
}
