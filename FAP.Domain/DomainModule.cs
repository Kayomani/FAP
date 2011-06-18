#region Copyright Kayomani 2011.  Licensed under the GPLv3 (Or later version), Expand for details. Do not remove this notice.

/**
    This program is free software: you can redistribute it and/or modify
    it under the terms of the GNU General Public License as published by
    the Free Software Foundation, either version 3 of the License, or any 
    later version.

    This program is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU General Public License for more details.

    You should have received a copy of the GNU General Public License
    along with this program.  If not, see <http://www.gnu.org/licenses/>.
 * */

#endregion

using Autofac;
using FAP.Domain.Entities;
using FAP.Domain.Handlers;
using FAP.Domain.Net;
using FAP.Domain.Services;

namespace FAP.Domain
{
    public class DomainModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterType<ShareInfoService>().SingleInstance();
            builder.RegisterType<ListenerService>();
            builder.RegisterType<Model>().SingleInstance();
            builder.RegisterType<HTTPHandler>();
            builder.RegisterType<LANPeerFinderService>().SingleInstance();
            builder.RegisterType<BufferService>().SingleInstance();
            builder.RegisterType<ServerUploadLimiterService>().SingleInstance();
            builder.RegisterType<LogService>().SingleInstance();
            builder.RegisterType<OverlordManagerService>().SingleInstance();
        }
    }
}