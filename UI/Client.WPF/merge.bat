mkdir merge
ilmerge /out:merge/Fap.Presentation.exe Fap.Presentation.exe Autofac.dll ContinuousLinq.dll LinqToWmi.Core.dll WpfApplicationFramework.dll Fap.Foundation.dll Fap.Network.dll Fap.Domain.dll Fap.Application.dll
copy Odyssey.dll merge
pause