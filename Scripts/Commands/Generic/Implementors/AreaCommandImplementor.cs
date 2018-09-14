using System;
using System.Collections;

namespace Server.Commands.Generic
{
	public class AreaCommandImplementor : BaseCommandImplementor
	{
		public static AreaCommandImplementor Instance { get; private set; }

		public AreaCommandImplementor()
		{
			Accessors = new[]{ "Area", "Group" };
			SupportRequirement = CommandSupport.Area;
			SupportsConditionals = true;
			AccessLevel = AccessLevel.GameMaster;
			Usage = "Area <command> [condition]";
			Description = "Invokes the command on all appropriate objects in a targeted area. Optional condition arguments can further restrict the set of objects.";

			Instance = this;
		}

		public override void Process( Mobile from, BaseCommand command, string[] args )
		{
			BoundingBoxPicker.Begin( from, OnTarget, new object[]{ command, args } );
		}

		public void OnTarget( Mobile from, Map map, Point3D start, Point3D end, object state )
		{
			try
			{
				object[] states = (object[])state;
				BaseCommand command = (BaseCommand)states[0];
				string[] args = (string[])states[1];

				Rectangle2D rect = new Rectangle2D( start.X, start.Y, end.X - start.X + 1, end.Y - start.Y + 1 );

				Extensions ext = Extensions.Parse( from, ref args );

				bool items, mobiles;

				if ( !CheckObjectTypes( from, command, ext, out items, out mobiles ) )
					return;

				IPooledEnumerable<IEntity> eable;

				if (items || mobiles)
					eable = map.GetObjectsInBounds(rect, items, mobiles);
				else
					return;

				ArrayList objs = new ArrayList();

				foreach ( IEntity obj in eable )
				{
					if ( mobiles && obj is Mobile && !BaseCommand.IsAccessible( from, obj ) )
						continue;

					if ( ext.IsValid( obj ) )
						objs.Add( obj );
				}

				eable.Free();

				ext.Filter( objs );

				RunCommand( from, objs, command, args );
			}
			catch ( Exception ex )
			{
				from.SendMessage( ex.Message );
			}
		}
	}
}
