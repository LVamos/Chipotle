﻿using Game.Entities;

namespace Game.Messaging.Events
{
	public class LocalityLeft: GameMessage
	{
		public readonly object Sender;
		public readonly Entity Entity;

		public LocalityLeft(object sender, Entity entity):base(sender)
		{
			this.Entity= entity;
		}
	}
}