﻿using System;
using System.Data;
using OTA;
using TDSM.Core.Data.Models;
using System.Linq;
using TDSM.Core;
using TDSM.Core.Data.Permissions;
using OTA.Permissions;
using OTA.Data;
using OTA.Data.Dapper.Extensions;
using Dapper.Contrib.Extensions;
using Dapper;
using OTA.Data.Dapper.Mappers;

namespace TDSM.Core.Data
{
#if DATA_CONNECTOR
	/// <summary>
	/// Direct access to the active Data Connector.
	/// </summary>
	/// <remarks>Plugins use this</remarks>
	public static class Storage
	{
		private static readonly object _sync = new object();
		private static IDataConnector _connector;

		/// <summary>
		/// Gets a value indicating if there is a connector available.
		/// </summary>
		/// <value><c>true</c> if is available; otherwise, <c>false</c>.</value>
		public static bool IsAvailable
		{
			get
			{ return _connector != null; }
		}

		/// <summary>
		/// Sets the active connector.
		/// </summary>
		/// <param name="connector">Connector.</param>
		/// <param name="throwWhenSet">If set to <c>true</c> and a connector has already been set an exception will be thrown.</param>
		public static void SetConnector(IDataConnector connector, bool throwWhenSet = true)
		{
			lock (_sync)
			{
				if (_connector != null && throwWhenSet)
				{
					throw new InvalidOperationException(String.Format("Attempted to load '{0}' when a '{1}' was already loaded", connector.ToString(), _connector.ToString()));
				}
				_connector = connector;
			}

			AuthenticatedUsers.Initialise();
			SettingsStore.Initialise();
		}

		/// <summary>
		/// Gets a builder compatible with the connector
		/// </summary>
		/// <returns>The builder.</returns>
		/// <param name="pluginName">Calling plugin name for encapsulation.</param>
		public static QueryBuilder GetBuilder(string pluginName)
		{
			if (_connector == null)
				throw new InvalidOperationException("No connector attached");
			return _connector.GetBuilder(pluginName);
		}

		//        public static QueryBuilder GetBuilder(string pluginName, string command, System.Data.CommandType type)
		//        {
		//            if (_connector == null)
		//                throw new InvalidOperationException("No connector attached");
		//            return _connector.GetBuilder(pluginName, command, type);
		//        }

		/// <summary>
		/// Execute the specified builder.
		/// </summary>
		/// <param name="builder">Builder.</param>
		public static bool Execute(QueryBuilder builder)
		{
			if (_connector == null)
				throw new InvalidOperationException("No connector attached");
			return _connector.Execute(builder);
		}

		/// <summary>
		/// Executes the builder and returns an insert id.
		/// </summary>
		/// <returns>The insert.</returns>
		/// <param name="builder">Builder.</param>
		public static long ExecuteInsert(QueryBuilder builder)
		{
			if (_connector == null)
				throw new InvalidOperationException("No connector attached");
			return _connector.ExecuteInsert(builder);
		}

		/// <summary>
		/// Executes a non query.
		/// </summary>
		/// <returns>The non query.</returns>
		/// <param name="builder">Builder.</param>
		public static int ExecuteNonQuery(QueryBuilder builder)
		{
			if (_connector == null)
				throw new InvalidOperationException("No connector attached");
			return _connector.ExecuteNonQuery(builder);
		}

		/// <summary>
		/// Executes the builder and returns the first row and column as a value.
		/// </summary>
		/// <returns>The scalar.</returns>
		/// <param name="builder">Builder.</param>
		/// <typeparam name="T">The 1st type parameter.</typeparam>
		public static T ExecuteScalar<T>(QueryBuilder builder)
		{
			if (_connector == null)
				throw new InvalidOperationException("No connector attached");
			return _connector.ExecuteScalar<T>(builder);
		}

		/// <summary>
		/// Executes and returns a data set.
		/// </summary>
		/// <returns>The data set.</returns>
		/// <param name="builder">Builder.</param>
		public static DataSet ExecuteDataSet(QueryBuilder builder)
		{
			if (_connector == null)
				throw new InvalidOperationException("No connector attached");
			return _connector.ExecuteDataSet(builder);
		}

		/// <summary>
		/// Executes and reflects rows into an array.
		/// </summary>
		/// <returns>The array.</returns>
		/// <param name="builder">Builder.</param>
		/// <typeparam name="T">The 1st type parameter.</typeparam>
		public static T[] ExecuteArray<T>(QueryBuilder builder) where T : new()
		{
			if (_connector == null)
				throw new InvalidOperationException("No connector attached");
			return _connector.ExecuteArray<T>(builder);
		}

		/// <summary>
		/// Determines if a player is permitted for a node
		/// </summary>
		/// <returns><c>true</c> if is permitted the specified node player; otherwise, <c>false</c>.</returns>
		/// <param name="node">Node.</param>
		/// <param name="player">Player.</param>
		public static Permission IsPermitted(string node, BasePlayer player)
		{
			if (_connector == null)
				return player.IsOp() ? Permission.Permitted : Permission.Denied;
			return _connector.IsPermitted(node, player);
		}

		/// <summary>
		/// Find a group by name
		/// </summary>
		/// <returns>The group.</returns>
		/// <param name="name">Name.</param>
		public static Group FindGroup(string name)
		{
			if (_connector == null)
				throw new InvalidOperationException("No connector attached");
			return _connector.FindGroup(name);
		}

		/// <summary>
		/// Add or update a group
		/// </summary>
		/// <returns><c>true</c>, if the update group was added/updated, <c>false</c> otherwise.</returns>
		/// <param name="name">Name.</param>
		/// <param name="applyToGuests">If set to <c>true</c>, the group will be applied to guests.</param>
		/// <param name="parent">Parent.</param>
		/// <param name="r">The red chat component.</param>
		/// <param name="g">The green chat component.</param>
		/// <param name="b">The blue chat component.</param>
		/// <param name="prefix">Prefix.</param>
		/// <param name="suffix">Suffix.</param>
		public static bool AddOrUpdateGroup(string name, bool applyToGuests = false, string parent = null, byte r = 255, byte g = 255, byte b = 255, string prefix = null, string suffix = null)
		{
			if (_connector == null)
				throw new InvalidOperationException("No connector attached");
			return _connector.AddOrUpdateGroup(name, applyToGuests, parent, r, g, b, prefix, suffix);
		}

		/// <summary>
		/// Remove a group
		/// </summary>
		/// <returns><c>true</c>, if group was removed, <c>false</c> otherwise.</returns>
		/// <param name="name">Name.</param>
		public static bool RemoveGroup(string name)
		{
			if (_connector == null)
				throw new InvalidOperationException("No connector attached");
			return _connector.RemoveGroup(name);
		}

		/// <summary>
		/// Adds a node to a group
		/// </summary>
		/// <returns><c>true</c>, if group node was added, <c>false</c> otherwise.</returns>
		/// <param name="groupName">Group name.</param>
		/// <param name="node">Node.</param>
		/// <param name="deny">If set to <c>true</c> deny.</param>
		public static bool AddGroupNode(string groupName, string node, Permission permission)
		{
			if (_connector == null)
				throw new InvalidOperationException("No connector attached");
			return _connector.AddGroupNode(groupName, node, permission);
		}

		/// <summary>
		/// Removes a node from a group
		/// </summary>
		/// <returns><c>true</c>, if group node was removed, <c>false</c> otherwise.</returns>
		/// <param name="groupName">Group name.</param>
		/// <param name="node">Node.</param>
		/// <param name="deny">If set to <c>true</c> deny.</param>
		public static bool RemoveGroupNode(string groupName, string node, Permission permission)
		{
			if (_connector == null)
				throw new InvalidOperationException("No connector attached");
			return _connector.RemoveGroupNode(groupName, node, permission);
		}

		/// <summary>
		/// Fetches the group names available
		/// </summary>
		/// <returns>The list.</returns>
		public static string[] GroupList()
		{
			if (_connector == null)
				throw new InvalidOperationException("No connector attached");
			return _connector.GroupList();
		}

		/// <summary>
		/// Fetches the nodes for a group
		/// </summary>
		/// <returns>The nodes.</returns>
		/// <param name="groupName">Group name.</param>
		public static NodePermission[] GroupNodes(string groupName)
		{
			if (_connector == null)
				throw new InvalidOperationException("No connector attached");
			return _connector.GroupNodes(groupName);
		}

		/// <summary>
		/// Adds a user to a group
		/// </summary>
		/// <returns><c>true</c>, if user to group was added, <c>false</c> otherwise.</returns>
		/// <param name="username">Username.</param>
		/// <param name="groupName">Group name.</param>
		public static bool AddUserToGroup(string username, string groupName)
		{
			if (_connector == null)
				throw new InvalidOperationException("No connector attached");
			return _connector.AddUserToGroup(username, groupName);
		}

		/// <summary>
		/// Removes a player from a group
		/// </summary>
		/// <returns><c>true</c>, if user from group was removed, <c>false</c> otherwise.</returns>
		/// <param name="username">Username.</param>
		/// <param name="groupName">Group name.</param>
		public static bool RemoveUserFromGroup(string username, string groupName)
		{
			if (_connector == null)
				throw new InvalidOperationException("No connector attached");
			return _connector.RemoveUserFromGroup(username, groupName);
		}

		/// <summary>
		/// Adds a specific node to a user
		/// </summary>
		/// <returns><c>true</c>, if node to user was added, <c>false</c> otherwise.</returns>
		/// <param name="username">Username.</param>
		/// <param name="node">Node.</param>
		/// <param name="deny">If set to <c>true</c> deny.</param>
		public static bool AddNodeToUser(string username, string node, Permission permission)
		{
			if (_connector == null)
				throw new InvalidOperationException("No connector attached");
			return _connector.AddNodeToUser(username, node, permission);
		}

		/// <summary>
		/// Removes a specific node from a user
		/// </summary>
		/// <returns><c>true</c>, if node from user was removed, <c>false</c> otherwise.</returns>
		/// <param name="username">Username.</param>
		/// <param name="node">Node.</param>
		/// <param name="deny">If set to <c>true</c> deny.</param>
		public static bool RemoveNodeFromUser(string username, string node, Permission permission)
		{
			if (_connector == null)
				throw new InvalidOperationException("No connector attached");
			return _connector.RemoveNodeFromUser(username, node, permission);
		}

		/// <summary>
		/// Fetches the associated groups names for a user
		/// </summary>
		/// <returns>The group list.</returns>
		/// <param name="username">Username.</param>
		public static string[] UserGroupList(string username)
		{
			if (_connector == null)
				throw new InvalidOperationException("No connector attached");
			return _connector.UserGroupList(username);
		}

		/// <summary>
		/// Fetches the list of nodes for a user
		/// </summary>
		/// <returns>The nodes.</returns>
		/// <param name="username">Username.</param>
		public static NodePermission[] UserNodes(string username)
		{
			if (_connector == null)
				throw new InvalidOperationException("No connector attached");
			return _connector.UserNodes(username);
		}

		/// <summary>
		/// Fetches the lower most group for a player
		/// </summary>
		/// <remarks>There should always be one at this stage in OTA. The flexibility is just here.</remarks>
		/// <returns>The inherited group for user.</returns>
		/// <param name="username">Username.</param>
		public static Group GetInheritedGroupForUser(string username)
		{
			if (_connector == null)
				throw new InvalidOperationException("No connector attached");
			return _connector.GetInheritedGroupForUser(username);
		}

		/// <summary>
		/// Gets the inherited group for a player.
		/// </summary>
		/// <returns>The inherited group for a player.</returns>
		/// <param name="player">Player.</param>
		public static Group GetInheritedGroup(BasePlayer player)
		{
			if (!IsAvailable)
				throw new InvalidOperationException("No connector attached");

			if (player.IsAuthenticated())
			{
				var grp = GetInheritedGroupForUser(player.name);
				if (grp != null) return grp;
			}

			var guestGroup = GetGuestGroup();
			if (guestGroup != null) return guestGroup;

			return null;
		}

		/// <summary>
		/// Gets the first guest group from the database
		/// </summary>
		/// <returns>The guest group.</returns>
		public static Group GetGuestGroup()
		{
			if (!IsAvailable)
				throw new InvalidOperationException("No connector attached");
			return _connector.GetGuestGroup();
		}
	}

#else
    /// <summary>
    /// Direct access to the active Data Connector.
    /// </summary>
    /// <remarks>Plugins use this</remarks>
    public static class Storage
    {
        //        private static readonly object _sync = new object();
        //        private static IDataConnector _connector;

        /// <summary>
        /// Gets a value indicating if there is a connector available.
        /// </summary>
        /// <value><c>true</c> if is available; otherwise, <c>false</c>.</value>
        public static bool IsAvailable
        {
            internal set;
            get;
            //            get
            //            {
            //                return OTAContext.HasConnection();
            //            }
        }

        /// <summary>
        /// Determines if a player is permitted for a node
        /// </summary>
        /// <returns><c>true</c> if is permitted the specified node player; otherwise, <c>false</c>.</returns>
        /// <param name="node">Node.</param>
        /// <param name="player">Player.</param>
        public static Permission IsPermitted(string node, BasePlayer player)
        {
            if (!IsAvailable)
                throw new InvalidOperationException("No connector attached");

            if (player != null)
            {
                var auth = player.GetAuthenticatedAs();
                if (!String.IsNullOrEmpty(auth))
                    return IsPermitted(node, false, auth);

                return IsPermitted(node, true);
            }

            return Permission.Denied;
        }

        private static Permission IsPermitted(string prmNode, bool prmIsGuest, string prmAuthentication = null)
        {
            if (!IsAvailable)
                throw new InvalidOperationException("No connector attached");

            var vPermissionValue = Permission.NoPermission;
            var vPlayerId = 0L;
            var vGroupId = 0L;
            var vPrevGroupId = 0L;
            var vNodeFound = false;

#if DAPPER
            using (var ctx = DatabaseFactory.CreateConnection())
#else
			using (var ctx = new TContext())
#endif
            {
                using (var txn = ctx.BeginTransaction())
                {
                    if (prmIsGuest == false && prmAuthentication != null && prmAuthentication.Length > 0)
                    {
                        var player = ctx.GetPlayer(prmAuthentication, transaction: txn).Single();
                        if (player != null)
                        {
                            vPlayerId = player.Id;

                            if (player.Operator)
                                return Permission.Permitted;
                        }

                        if (vPlayerId > 0)
                        {
                            /*
                            If the user has specific nodes then use them
                            If not then search for a group
                            If still none then try the guest permissions
                        */

                            /*Do we have any nodes?*/
                            var nodes = ctx.GetPermissionByNodeForPlayer(vPlayerId, prmNode, transaction: txn);
                            if (nodes != null && nodes.Count() > 0)
                            {
                                if (nodes.Any(x => x.Node == "*") || nodes.Where(x => x.Permission == Permission.Denied).Count() == 0)
                                {
                                    vPermissionValue = Permission.Permitted;
                                    vNodeFound = true;
                                }
                                else
                                {
                                    vPermissionValue = Permission.Denied;
                                    vNodeFound = true;
                                }
                            }
                            else
                            {
                                /*
                                For each group, see if it has a permission
                                Else, if it has a parent recheck.
                                Else guestMode
                            */

                                var grp = ctx.GetPlayerGroups(vPlayerId, transaction: txn).FirstOrDefault();
                                vGroupId = 0;
                                if (grp != null) vGroupId = grp.Id;
                                if (vGroupId > 0)
                                {
                                    vPrevGroupId = vGroupId;
                                    vNodeFound = false;

                                    while (vGroupId > 0 && !vNodeFound)
                                    {
                                        /* Check group permissions */

                                        var groupPermissions = ctx.GetPermissionByNodeForGroup(vGroupId, prmNode, transaction: txn);

                                        if
                                        (
                                            groupPermissions.Any(x => x.Node == "*" && x.Permission == Permission.Permitted) || 
                                            groupPermissions.Where(x => x.Permission == Permission.Permitted).Count() > 0
                                        )
                                        {
                                            vPermissionValue = Permission.Permitted;
                                            vGroupId = 0;
                                            vNodeFound = true;
                                        }
                                        else if (groupPermissions.Where(x => x.Permission == Permission.Denied).Count() > 0)
                                        {
                                            vPermissionValue = Permission.Denied;
                                            vGroupId = 0;
                                            vNodeFound = true;
                                        }
                                        else
                                        {
                                            var par = ctx.GetParentForGroup(vGroupId, transaction: txn).FirstOrDefault();
                                            if (par != null)
                                            {
                                                vGroupId = par.Id;
                                                if (vPrevGroupId == vGroupId)
                                                {
                                                    vGroupId = 0;
                                                }

                                                vPrevGroupId = vGroupId;
                                            }
                                            else
                                            {
                                                vGroupId = 0;
                                            }
                                        }
                                    }
                                }
                                if (!vNodeFound)
                                {
                                    prmIsGuest = true;
                                }
                            }
                        }
                        else
                        {
                            /* Invalid user - try guest */
                            prmIsGuest = true;
                        }
                    }

                    if (!vNodeFound && prmIsGuest)
                    {
                        if (ctx.GuestGroupHasNode(prmNode, Permission.Permitted, transaction: txn))
                        {
                            vPermissionValue = Permission.Permitted;
                            vNodeFound = true;
                        }
                    }

                    return vPermissionValue;
                }
            }
        }

        /// <summary>
        /// Find a group by name
        /// </summary>
        /// <returns>The group.</returns>
        /// <param name="name">Name.</param>
        public static Group FindGroup(string name)
        {
            if (!IsAvailable)
                throw new InvalidOperationException("No connector attached");

#if DAPPER
            using (var ctx = DatabaseFactory.CreateConnection())
            {
                using (var txn = ctx.BeginTransaction())
                    return ctx.SingleOrDefault<Group>(new { Name = name }, transaction: txn);
            }
#else
			using (var ctx = new TContext()) return ctx.Groups.SingleOrDefault(x => x.Name == name);
#endif
        }

        /// <summary>
        /// Add or update a group
        /// </summary>
        /// <returns><c>true</c>, if the update group was added/updated, <c>false</c> otherwise.</returns>
        /// <param name="name">Name.</param>
        /// <param name="applyToGuests">If set to <c>true</c>, the group will be applied to guests.</param>
        /// <param name="parent">Parent.</param>
        /// <param name="r">The red chat component.</param>
        /// <param name="g">The green chat component.</param>
        /// <param name="b">The blue chat component.</param>
        /// <param name="prefix">Prefix.</param>
        /// <param name="suffix">Suffix.</param>
        public static Group AddOrUpdateGroup(string name, bool applyToGuests = false, string parent = null, byte r = 255, byte g = 255, byte b = 255, string prefix = null, string suffix = null)
        {
            if (!IsAvailable)
                throw new InvalidOperationException("No connector attached");

#if DAPPER
            using (var ctx = DatabaseFactory.CreateConnection())
            {
                using (var txn = ctx.BeginTransaction())
                {
                    var group = ctx.SingleOrDefault<Group>(new { Name = name }, transaction: txn);
                    if (group != null)
                    {
                        group.ApplyToGuests = applyToGuests;
                        group.Parent = parent;
                        group.Chat_Red = r;
                        group.Chat_Green = g;
                        group.Chat_Blue = b;
                        group.Chat_Prefix = prefix;
                        group.Chat_Suffix = suffix;

                        ctx.Update(group, transaction: txn);
                    }
                    else
                    {
                        group = new Group()
                        {
                            Name = name,
                            ApplyToGuests = applyToGuests,
                            Parent = parent,
                            Chat_Red = r,
                            Chat_Green = g,
                            Chat_Blue = b,
                            Chat_Prefix = prefix,
                            Chat_Suffix = suffix
                        };
                        group.Id = ctx.Insert(group, transaction: txn);
                    }

                    txn.Commit();
                    return group;
                }
            }
#else
			using (var ctx = new TContext())
			{
				var group = ctx.Groups.SingleOrDefault(x => x.Name == name);
				if (group != null)
				{
					group.ApplyToGuests = applyToGuests;
					group.Parent = parent;
					group.Chat_Red = r;
					group.Chat_Green = g;
					group.Chat_Blue = b;
					group.Chat_Prefix = prefix;
					group.Chat_Suffix = suffix;
				}
				else
				{
					ctx.Groups.Add(group = new Group()
					{
						Name = name,
						ApplyToGuests = applyToGuests,
						Parent = parent,
						Chat_Red = r,
						Chat_Green = g,
						Chat_Blue = b,
						Chat_Prefix = prefix,
						Chat_Suffix = suffix
					});
				}

				ctx.SaveChanges();

			return group;
		}
#endif
        }

        public static PermissionNode FindOrCreateNode(string node, Permission permission)
        {
            if (!IsAvailable)
                throw new InvalidOperationException("No connector attached");

#if DAPPER
            using (var ctx = DatabaseFactory.CreateConnection())
            {
                using (var txn = ctx.BeginTransaction())
                {
                    var existing = ctx.SingleOrDefault<PermissionNode>(new { Node = node, Permission = permission }, transaction: txn);
                    if (existing != null) return existing;
                    else
                    {
                        existing = new PermissionNode()
                        {
                            Node = node,
                            Permission = permission
                        };
                        existing.Id = ctx.Insert(existing, transaction: txn);

                        txn.Commit();
                        return existing;
                    }
                }
            }
#else
			using (var ctx = new TContext())
			{
				var existing = ctx.Nodes.SingleOrDefault(x => x.Node == node && x.Permission == permission);
				if (existing != null) return existing;
				else
				{
					ctx.Nodes.Add(existing = new NodePermission()
					{
						Node = node,
						Permission = permission
					});

					ctx.SaveChanges();

					return existing;
				}
			}
#endif
        }

        /// <summary>
        /// Remove a group
        /// </summary>
        /// <returns><c>true</c>, if group was removed, <c>false</c> otherwise.</returns>
        /// <param name="name">Name.</param>
        public static bool RemoveGroup(string name)
        {
            if (!IsAvailable)
                throw new InvalidOperationException("No connector attached");

#if DAPPER
            using (var ctx = DatabaseFactory.CreateConnection())
            {
                using (var txn = ctx.BeginTransaction())
                {
                    var res = ctx.Delete<Group>(new { Name = name }, transaction: txn) > 0;
                    txn.Commit();
                    return res;
                }
            }
#else
			using (var ctx = new TContext())
			{
				ctx.Groups.RemoveRange(ctx.Groups.Where(x => x.Name == name));
				ctx.SaveChanges();

				return !ctx.Groups.Any(x => x.Name == name);
			}
#endif
        }

        /// <summary>
        /// Adds a node to a group
        /// </summary>
        /// <returns><c>true</c>, if group node was added, <c>false</c> otherwise.</returns>
        /// <param name="groupName">Group name.</param>
        /// <param name="node">Node.</param>
        /// <param name="deny">If set to <c>true</c> deny.</param>
        public static bool AddGroupNode(string groupName, string node, Permission permission)
        {
            if (!IsAvailable)
                throw new InvalidOperationException("No connector attached");

#if DAPPER
            var perm = FindOrCreateNode(node, permission);
            using (var ctx = DatabaseFactory.CreateConnection())
            {
                using (var txn = ctx.BeginTransaction())
                {
                    var group = ctx.SingleOrDefault<Group>(new { Name = groupName }, transaction: txn);
                    ctx.Insert(new GroupNode()
                    {
                        GroupId = group.Id,
                        NodeId = perm.Id
                    }, transaction: txn);

                    txn.Commit();
                    return true;
                }
            }
#else
			using (var ctx = new TContext())
			{
				var group = ctx.Groups.Where(x => x.Name == groupName).SingleOrDefault();
				var perm = FindOrCreateNode(node, permission);

				ctx.GroupNodes.Add(new GroupNode()
				{
					GroupId = group.Id,
					NodeId = perm.Id
				});

				ctx.SaveChanges();

				return true;
			}
#endif
        }

        /// <summary>
        /// Removes a node from a group
        /// </summary>
        /// <returns><c>true</c>, if group node was removed, <c>false</c> otherwise.</returns>
        /// <param name="groupName">Group name.</param>
        /// <param name="node">Node.</param>
        /// <param name="deny">If set to <c>true</c> deny.</param>
        public static bool RemoveGroupNode(string groupName, string node, Permission permission)
        {
            if (!IsAvailable)
                throw new InvalidOperationException("No connector attached");

#if DAPPER
            var group = FindGroup(groupName);
            if (group == null) return false;
            using (var ctx = DatabaseFactory.CreateConnection())
            {
                using (var txn = ctx.BeginTransaction())
                {
                    var res = ctx.Execute($"delete from {TableMapper.TypeToName<GroupNode>()} where {ColumnMapper.Enclose("Id")} in ( select Id from (" +
                        $"select gn.{ColumnMapper.Enclose("Id")} from {TableMapper.TypeToName<GroupNode>()} gn " +
                            $"inner join {TableMapper.TypeToName<PermissionNode>()} nd on gn.{ColumnMapper.Enclose("NodeId")} = nd.{ColumnMapper.Enclose("Id")} " +
                        $"where gn.{ColumnMapper.Enclose("GroupId")} = @GroupId and nd.{ColumnMapper.Enclose("Node")} = @Node and nd.{ColumnMapper.Enclose("Permission")} = @Permission" +
                    ") sq )",
                    new { GroupId = group.Id, Node = node, Permission = (int)permission }, transaction: txn) > 0;
                    txn.Commit();
                    return res;
                }
            }
#else
			using (var ctx = new TContext())
			{
				ctx.GroupNodes.RemoveRange(
								from grp in ctx.Groups
								join nds in ctx.GroupNodes on grp.Id equals nds.GroupId
								select nds
							);

				ctx.SaveChanges();

				return (from grp in ctx.Groups
						join nds in ctx.GroupNodes on grp.Id equals nds.GroupId
						select nds).Any();
			}
#endif
        }

        /// <summary>
        /// Fetches the group names available
        /// </summary>
        /// <returns>The list.</returns>
        public static string[] GroupList()
        {
            if (!IsAvailable)
                throw new InvalidOperationException("No connector attached");

#if DAPPER
            using (var ctx = DatabaseFactory.CreateConnection())
            {
                using (var txn = ctx.BeginTransaction())
                    return ctx.Query<String>($"select Name from {TableMapper.TypeToName<Group>()}", transaction: txn).ToArray();
            }
#else
			using (var ctx = new TContext())
			{
				return ctx.Groups.Select(x => x.Name).ToArray();
			}
#endif
        }

        /// <summary>
        /// Fetches the nodes for a group
        /// </summary>
        /// <returns>The nodes.</returns>
        /// <param name="groupName">Group name.</param>
        public static PermissionNode[] GroupNodes(string groupName)
        {
            if (!IsAvailable)
                throw new InvalidOperationException("No connector attached");

#if DAPPER
            using (var ctx = DatabaseFactory.CreateConnection())
            {
                using (var txn = ctx.BeginTransaction())
                    return ctx.Query<PermissionNode>($"select nd.* from {TableMapper.TypeToName<Group>()} g " +
                    $"inner join {TableMapper.TypeToName<GroupNode>()} gn on g.{ColumnMapper.Enclose("Id")} = gn.{ColumnMapper.Enclose("GroupId")} " +
                    $"inner join {TableMapper.TypeToName<PermissionNode>()} nd on gn.{ColumnMapper.Enclose("NodeId")} = nd.{ColumnMapper.Enclose("Id")} " +
                    $"where g.{ColumnMapper.Enclose("Name")} = @GroupName", new { GroupName = groupName }, transaction: txn).ToArray();
            }
#else
			using (var ctx = new TContext())
			{
				return ctx.Groups
					.Where(g => g.Name == groupName)
					.Join(ctx.GroupNodes, grp => grp.Id, gn => gn.GroupId, (a, b) => b)
					.Join(ctx.Nodes, gp => gp.Id, nd => nd.Id, (a, b) => b)
					.ToArray();
			}
#endif
        }

        /// <summary>
        /// Adds a user to a group
        /// </summary>
        /// <returns><c>true</c>, if user to group was added, <c>false</c> otherwise.</returns>
        /// <param name="username">Username.</param>
        /// <param name="groupName">Group name.</param>
        public static bool AddUserToGroup(string username, string groupName)
        {
            if (!IsAvailable)
                throw new InvalidOperationException("No connector attached");

#if DAPPER
            using (var ctx = DatabaseFactory.CreateConnection())
            {
                using (var txn = ctx.BeginTransaction())
                {
                    var player = ctx.Single<DbPlayer>(new { Name = username }, transaction: txn);
                    var group = ctx.Single<Group>(new { Name = groupName }, transaction: txn);

                    //Temporary until the need for more than one group
                    if (ctx.Where<PlayerGroup>(new { PlayerId = player.Id }, transaction: txn).Any(x => x.GroupId > 0))
                        throw new NotSupportedException("A player can only be associated to one group, please assign a parent to the desired group");

                    ctx.Insert(new PlayerGroup()
                    {
                        GroupId = group.Id,
                        PlayerId = player.Id
                    }, transaction: txn);

                    txn.Commit();
                    return true;
                }
            }
#else
			using (var ctx = new TContext())
			{
				var user = ctx.Players.Single(x => x.Name == username);
				var group = ctx.Groups.Single(x => x.Name == groupName);

				//Temporary until the need for more than one group
				if (ctx.PlayerGroups.Any(x => x.PlayerId == user.Id && x.GroupId > 0))
					throw new NotSupportedException("A player can only be associated to one group, please assign a parent to the desired group");

				ctx.PlayerGroups.Add(new PlayerGroup()
				{
					GroupId = group.Id,
					UserId = user.Id
				});

				ctx.SaveChanges();

				return true;
			}
#endif
        }

        /// <summary>
        /// Removes a player from a group
        /// </summary>
        /// <returns><c>true</c>, if user from group was removed, <c>false</c> otherwise.</returns>
        /// <param name="username">Username.</param>
        /// <param name="groupName">Group name.</param>
        public static bool RemoveUserFromGroup(string username, string groupName)
        {
            if (!IsAvailable)
                throw new InvalidOperationException("No connector attached");

#if DAPPER
            using (var ctx = DatabaseFactory.CreateConnection())
            {
                using (var txn = ctx.BeginTransaction())
                {
                    var player = ctx.Single<DbPlayer>(new { Name = username }, transaction: txn);
                    var group = ctx.Single<Group>(new { Name = groupName }, transaction: txn);

                    var res = ctx.Delete<PlayerGroup>(new { PlayerId = player.Id, GroupId = group.Id }, transaction: txn) > 0;
                    txn.Commit();
                    return res;
                }
            }
#else
			using (var ctx = new TContext())
			{
				var user = ctx.Players.Single(x => x.Name == username);
				var group = ctx.Groups.Single(x => x.Name == groupName);

				ctx.PlayerGroups.RemoveRange(ctx.PlayerGroups.Where(x =>
					x.GroupId == group.Id &&
					x.UserId == user.Id
				));

				ctx.SaveChanges();

				return ctx.PlayerGroups.Any(x =>
					x.GroupId == group.Id &&
					x.UserId == user.Id
				);
			}
#endif
        }

        /// <summary>
        /// Adds a specific node to a user
        /// </summary>
        /// <returns><c>true</c>, if node to user was added, <c>false</c> otherwise.</returns>
        /// <param name="username">Username.</param>
        /// <param name="node">Node.</param>
        /// <param name="deny">If set to <c>true</c> deny.</param>
        public static bool AddNodeToUser(string username, string node, Permission permission)
        {
            if (!IsAvailable)
                throw new InvalidOperationException("No connector attached");

#if DAPPER
            var perm = FindOrCreateNode(node, permission);
            using (var ctx = DatabaseFactory.CreateConnection())
            {
                using (var txn = ctx.BeginTransaction())
                {
                    var player = ctx.Single<DbPlayer>(new { Name = username }, transaction: txn);

                    if (ctx.Any<PlayerNode>(new { PlayerId = player.Id, NodeId = perm.Id }, transaction: txn))
                        return false;

                    var res = ctx.Insert(new PlayerNode()
                    {
                        NodeId = perm.Id,
                        PlayerId = player.Id
                    }, transaction: txn) > 0;
                    txn.Commit();
                    return res;
                }
            }
#else
			using (var ctx = new TContext())
			{
				var user = ctx.Players.Single(x => x.Name == username);
				var perm = FindOrCreateNode(node, permission);

			//TODO (EF) WRONG! Looks like it was copied code.
				//ctx.PlayerNodes.RemoveRange(ctx.PlayerNodes.Where(x =>
				//    x.NodeId == perm.Id &&
				//    x.UserId == user.Id
				//));

				//ctx.SaveChanges();

				//return ctx.PlayerNodes.Any(x =>
				//    x.NodeId == perm.Id &&
				//    x.UserId == user.Id
				//);
			}
#endif
        }

        /// <summary>
        /// Removes a specific node from a user
        /// </summary>
        /// <returns><c>true</c>, if node from user was removed, <c>false</c> otherwise.</returns>
        /// <param name="username">Username.</param>
        /// <param name="node">Node.</param>
        /// <param name="deny">If set to <c>true</c> deny.</param>
        public static bool RemoveNodeFromUser(string username, string node, Permission permission)
        {
            if (!IsAvailable)
                throw new InvalidOperationException("No connector attached");

#if DAPPER
            using (var ctx = DatabaseFactory.CreateConnection())
            {
                using (var txn = ctx.BeginTransaction())
                {
                    var player = ctx.Single<DbPlayer>(new { Name = username }, transaction: txn);

                    var res = ctx.Execute($"delete from {TableMapper.TypeToName<PlayerNode>()} where {TableMapper.TypeToName<PlayerNode>()}.{ColumnMapper.Enclose("Id")} in ( select Id from (" +
                            $"select pn.{ColumnMapper.Enclose("Id")} from {TableMapper.TypeToName<PlayerNode>()} pn " +
                                $"inner join {TableMapper.TypeToName<PermissionNode>()} nd on pn.{ColumnMapper.Enclose("NodeId")} = nd.{ColumnMapper.Enclose("Id")} " +
                            $"where pn.{ColumnMapper.Enclose("PlayerId")} = @PlayerId and nd.{ColumnMapper.Enclose("Node")} = @Node and nd.{ColumnMapper.Enclose("Permission")} = @Permission" +
                        ") sq)",
                        new { PlayerId = player.Id, Node = node, Permission = (int)permission }, transaction: txn) > 0;
                    txn.Commit();
                    return res;
                }
            }
#else
			using (var ctx = new TContext())
			{
				ctx.PlayerNodes.RemoveRange(ctx.Players
					.Where(p => p.Name == username)
					.Join(ctx.PlayerNodes, x => x.Id, y => y.UserId, (a, b) => b)
				);

				ctx.SaveChanges();

				return ctx.Players
					.Where(p => p.Name == username)
					.Join(ctx.PlayerNodes, x => x.Id, y => y.UserId, (a, b) => b)
					.Any();
			}
#endif
        }

        /// <summary>
        /// Fetches the associated groups names for a user
        /// </summary>
        /// <returns>The group list.</returns>
        /// <param name="username">Username.</param>
        public static string[] UserGroupList(string username)
        {
            if (!IsAvailable)
                throw new InvalidOperationException("No connector attached");

#if DAPPER
            using (var ctx = DatabaseFactory.CreateConnection())
            {
                using (var txn = ctx.BeginTransaction())
                {
                    return ctx.Query<string>($"select g.{ColumnMapper.Enclose("Name")} from {TableMapper.TypeToName<DbPlayer>()} p " +
                        $"inner join {TableMapper.TypeToName<PlayerGroup>()} pg on p.{ColumnMapper.Enclose("Id")} = pg.{ColumnMapper.Enclose("PlayerId")} " +
                        $"inner join {TableMapper.TypeToName<Group>()} g on pg.{ColumnMapper.Enclose("GroupId")} = g.{ColumnMapper.Enclose("Id")} " +
                        $"where p.{ColumnMapper.Enclose("Name")} = @PlayerName",
                        new { PlayerName = username }, transaction: txn).ToArray();
                }
            }
#else
			using (var ctx = new TContext())
			{
				return ctx.Players
					.Where(p => p.Name == username)
					.Join(ctx.PlayerGroups, pg => pg.Id, y => y.UserId, (a, b) => b)
					.Join(ctx.Groups, pg => pg.Id, g => g.Id, (a, b) => b)
					.Select(x => x.Name)
					.ToArray();
			}
#endif
        }

        /// <summary>
        /// Fetches the list of nodes for a user
        /// </summary>
        /// <returns>The nodes.</returns>
        /// <param name="username">Username.</param>
        public static PermissionNode[] UserNodes(string username)
        {
            if (!IsAvailable)
                throw new InvalidOperationException("No connector attached");

#if DAPPER
            using (var ctx = DatabaseFactory.CreateConnection())
            {
                using (var txn = ctx.BeginTransaction())
                {
                    return ctx.Query<PermissionNode>($"select n.* from {TableMapper.TypeToName<DbPlayer>()} p " +
                        $"inner join {TableMapper.TypeToName<PlayerNode>()} pn on p.{ColumnMapper.Enclose("Id")} = pn.{ColumnMapper.Enclose("PlayerId")} " +
                        $"inner join {TableMapper.TypeToName<PermissionNode>()} n on pn.{ColumnMapper.Enclose("NodeId")} = n.{ColumnMapper.Enclose("Id")} " +
                        $"where p.{ColumnMapper.Enclose("Name")} = @PlayerName",
                        new { PlayerName = username }, transaction: txn).ToArray();
                }
            }
#else
			using (var ctx = new TContext())
			{
				return ctx.Players
					.Where(p => p.Name == username)
					.Join(ctx.PlayerNodes, pn => pn.Id, y => y.UserId, (a, b) => b)
					.Join(ctx.Nodes, pn => pn.Id, nd => nd.Id, (a, b) => b)
					.ToArray();
			}
#endif
        }

        /// <summary>
        /// Fetches the lower most group for a player
        /// </summary>
        /// <remarks>There should always be one at this stage in OTA. The flexibility is just here.</remarks>
        /// <returns>The inherited group for user.</returns>
        /// <param name="username">Username.</param>
        public static Group GetInheritedGroupForUser(string username)
        {
            if (!IsAvailable)
                throw new InvalidOperationException("No connector attached");

#if DAPPER
            using (var ctx = DatabaseFactory.CreateConnection())
            {
                using (var txn = ctx.BeginTransaction())
                {
                    return ctx.QueryFirstOrDefault<Group>($"select g.* from {TableMapper.TypeToName<DbPlayer>()} p " +
                    $"inner join {TableMapper.TypeToName<PlayerGroup>()} pg on p.{ColumnMapper.Enclose("Id")} = pg.{ColumnMapper.Enclose("PlayerId")} " +
                    $"inner join {TableMapper.TypeToName<Group>()} g on pg.{ColumnMapper.Enclose("GroupId")} = g.{ColumnMapper.Enclose("Id")} " +
                    $"where p.{ColumnMapper.Enclose("Name")} = @PlayerName order by pg.{ColumnMapper.Enclose("Id")}",
                    new { PlayerName = username }, transaction: txn);
                }
            }
#else
				using (var ctx = new TContext())
			{
				return ctx.Players
					.Where(x => x.Name == username)
					.Join(ctx.PlayerGroups, pg => pg.Id, us => us.UserId, (a, b) => b)
					.Join(ctx.Groups, pg => pg.GroupId, gr => gr.Id, (a, b) => b)
					.FirstOrDefault();
			}
#endif
        }

        /// <summary>
        /// Gets the inherited group for a player.
        /// </summary>
        /// <returns>The inherited group for a player.</returns>
        /// <param name="player">Player.</param>
        public static Group GetInheritedGroup(BasePlayer player)
        {
            if (!IsAvailable)
                throw new InvalidOperationException("No connector attached");

            if (player.IsAuthenticated())
            {
                var grp = GetInheritedGroupForUser(player.name);
                if (grp != null) return grp;
            }

            var guestGroup = GetGuestGroup();
            if (guestGroup != null) return guestGroup;

            return null;
        }

        /// <summary>
        /// Gets the first guest group from the database
        /// </summary>
        /// <returns>The guest group.</returns>
        public static Group GetGuestGroup()
        {
            if (!IsAvailable)
                throw new InvalidOperationException("No connector attached");

#if DAPPER
            using (var ctx = DatabaseFactory.CreateConnection())
            {
                using (var txn = ctx.BeginTransaction())
                    return ctx.FirstOrDefault<Group>(new { ApplyToGuests = true }, transaction: txn);
            }
#else
			using (var ctx = new TContext())
			{
				return ctx.Groups.Where(x => x.ApplyToGuests).FirstOrDefault();
			}
#endif
        }
    }
#endif
}

