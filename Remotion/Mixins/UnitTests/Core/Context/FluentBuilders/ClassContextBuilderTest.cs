// This file is part of the re-motion Core Framework (www.re-motion.org)
// Copyright (c) rubicon IT GmbH, www.rubicon.eu
// 
// The re-motion Core Framework is free software; you can redistribute it 
// and/or modify it under the terms of the GNU Lesser General Public License 
// as published by the Free Software Foundation; either version 2.1 of the 
// License, or (at your option) any later version.
// 
// re-motion is distributed in the hope that it will be useful, 
// but WITHOUT ANY WARRANTY; without even the implied warranty of 
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the 
// GNU Lesser General Public License for more details.
// 
// You should have received a copy of the GNU Lesser General Public License
// along with re-motion; if not, see http://www.gnu.org/licenses.
// 
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using Moq;
using NUnit.Framework;
using Remotion.Mixins.Context;
using Remotion.Mixins.Context.FluentBuilders;
using Remotion.Mixins.Context.Suppression;
using Remotion.Mixins.UnitTests.Core.TestDomain;

namespace Remotion.Mixins.UnitTests.Core.Context.FluentBuilders
{
  [TestFixture]
  public class ClassContextBuilderTest
  {
    private Mock<MixinConfigurationBuilder> _parentBuilderMock;
    private ClassContextBuilder _classBuilder;
    private Mock<ClassContextBuilder> _classBuilderMock;
    private Mock<MixinContextBuilder> _mixinBuilderMock;

    [SetUp]
    public void SetUp ()
    {
      _parentBuilderMock = new Mock<MixinConfigurationBuilder>(MockBehavior.Strict, (MixinConfiguration)null);
      _classBuilder = new ClassContextBuilder(_parentBuilderMock.Object, typeof(BaseType2));
      _classBuilderMock = new Mock<ClassContextBuilder>(MockBehavior.Strict, _parentBuilderMock.Object, typeof(BaseType2));
      _mixinBuilderMock = new Mock<MixinContextBuilder>(
          MockBehavior.Strict,
          _classBuilderMock.Object,
          typeof(BT2Mixin1),
          MixinContextOriginObjectMother.Create());
    }

    private Type[] GetMixinTypes ()
    {
      return GetMixinTypes(_classBuilder);
    }

    private Type[] GetMixinTypes (ClassContextBuilder classBuilder)
    {
      return classBuilder.MixinContextBuilders.Select(mcb => mcb.MixinType).ToArray();
    }

    [Test]
    public void Initialization_Standalone ()
    {
      var classBuilder = new ClassContextBuilder(typeof(BaseType2));
      Assert.That(classBuilder.TargetType, Is.SameAs(typeof(BaseType2)));
      Assert.That(classBuilder.Parent, Is.Not.Null);
      Assert.That(_classBuilder.MixinContextBuilders, Is.Empty);
      Assert.That(_classBuilder.ComposedInterfaces.ToArray(), Is.Empty);

      ClassContext classContext = _classBuilder.BuildClassContext(new ClassContext[0]);
      Assert.That(classContext.Mixins.Count, Is.EqualTo(0));
      Assert.That(classContext.ComposedInterfaces.Count, Is.EqualTo(0));
    }

    [Test]
    public void Initialization_WithNoParentContext ()
    {
      Assert.That(_classBuilder.TargetType, Is.SameAs(typeof(BaseType2)));
      Assert.That(_classBuilder.Parent, Is.SameAs(_parentBuilderMock.Object));
      Assert.That(_classBuilder.MixinContextBuilders, Is.Empty);
      Assert.That(_classBuilder.ComposedInterfaces.ToArray(), Is.Empty);

      ClassContext classContext = _classBuilder.BuildClassContext(new ClassContext[0]);
      Assert.That(classContext.Mixins.Count, Is.EqualTo(0));
      Assert.That(classContext.ComposedInterfaces.Count, Is.EqualTo(0));
    }

    [Test]
    public void Clear ()
    {
      var classBuilder = new ClassContextBuilder(_parentBuilderMock.Object, typeof(BaseType1));
      classBuilder.AddMixin<BT1Mixin2>();
      classBuilder.AddComposedInterface<IBaseType31>();

      Assert.That(classBuilder.MixinContextBuilders, Is.Not.Empty);
      Assert.That(classBuilder.ComposedInterfaces, Is.Not.Empty);
      Assert.That(classBuilder.SuppressInheritance, Is.False);

      Assert.That(classBuilder.Clear(), Is.SameAs(classBuilder));
      Assert.That(classBuilder.MixinContextBuilders, Is.Empty);
      Assert.That(classBuilder.ComposedInterfaces.ToArray(), Is.Empty);
      Assert.That(classBuilder.SuppressInheritance, Is.True);
    }

    [Test]
    public void AddMixin_NonGeneric ()
    {
      var origin = MixinContextOriginObjectMother.Create();
      MixinContextBuilder mixinBuilder = _classBuilder.AddMixin(typeof(BT2Mixin1), origin);

      Assert.That(mixinBuilder.MixinType, Is.SameAs(typeof(BT2Mixin1)));
      Assert.That(mixinBuilder.Parent, Is.SameAs(_classBuilder));
      Assert.That(mixinBuilder.Origin, Is.SameAs(origin));
      Assert.That(_classBuilder.MixinContextBuilders, Has.Member(mixinBuilder));
    }

    [Test]
    [MethodImpl(MethodImplOptions.NoInlining)]
    public void AddMixin_NonGeneric_WithoutOrigin ()
    {
      var expectedOrigin = MixinContextOrigin.CreateForMethod(MethodBase.GetCurrentMethod());

      _classBuilderMock.Setup(mock => mock.AddMixin(typeof(BT2Mixin1), expectedOrigin)).Returns(_mixinBuilderMock.Object).Verifiable();

      Assert.That(_classBuilderMock.Object.AddMixin(typeof(BT2Mixin1)), Is.SameAs(_mixinBuilderMock.Object));
      _classBuilderMock.Verify();
    }

    [Test]
    public void AddMixin_Twice ()
    {
      Assert.That(
          () => _classBuilder.AddMixin(typeof(BT2Mixin1)).AddMixin(typeof(BT2Mixin1)),
          Throws.ArgumentException
              .With.Message.Contains(
                  "Remotion.Mixins.UnitTests.Core.TestDomain.BT2Mixin1 is already configured as a "
                  + "mixin for type Remotion.Mixins.UnitTests.Core.TestDomain.BaseType2."));
    }

    [Test]
    public void AddMixin_Twice_Generic1 ()
    {
      Assert.That(
          () => _classBuilder.AddMixin(typeof(GenericMixinWithVirtualMethod<>)).AddMixin(typeof(GenericMixinWithVirtualMethod<>)),
          Throws.ArgumentException
              .With.Message.Contains(
                  "Remotion.Mixins.UnitTests.Core.TestDomain.GenericMixinWithVirtualMethod`1 is "
                  + "already configured as a mixin for type Remotion.Mixins.UnitTests.Core.TestDomain.BaseType2."));
    }

    [Test]
    public void AddMixin_Twice_Generic2 ()
    {
      Assert.That(
          () => _classBuilder.AddMixin(typeof(GenericMixinWithVirtualMethod<object>)).AddMixin(typeof(GenericMixinWithVirtualMethod<>)),
          Throws.ArgumentException
              .With.Message.Contains(
                  "Remotion.Mixins.UnitTests.Core.TestDomain.GenericMixinWithVirtualMethod`1 is "
                  + "already configured as a mixin for type Remotion.Mixins.UnitTests.Core.TestDomain.BaseType2."));
    }

    [Test]
    public void AddMixin_Twice_Generic3 ()
    {
      Assert.That(
          () => _classBuilder.AddMixin(typeof(GenericMixinWithVirtualMethod<>)).AddMixin(typeof(GenericMixinWithVirtualMethod<object>)),
          Throws.ArgumentException
              .With.Message.Contains(
                  "Remotion.Mixins.UnitTests.Core.TestDomain.GenericMixinWithVirtualMethod`1 is "
                  + "already configured as a mixin for type Remotion.Mixins.UnitTests.Core.TestDomain.BaseType2."));
    }

    [Test]
    public void AddMixin_Twice_Generic4 ()
    {
      Assert.That(
          () => _classBuilder.AddMixin(typeof(GenericMixinWithVirtualMethod<string>)).AddMixin(typeof(GenericMixinWithVirtualMethod<object>)),
          Throws.ArgumentException
              .With.Message.Contains(
                  "Remotion.Mixins.UnitTests.Core.TestDomain.GenericMixinWithVirtualMethod`1 is "
                  + "already configured as a mixin for type Remotion.Mixins.UnitTests.Core.TestDomain.BaseType2."));
    }

    [Test]
    public void AddMixin_Generic ()
    {
      var origin = MixinContextOriginObjectMother.Create();

      _classBuilderMock.Setup(mock => mock.AddMixin<BT2Mixin1>(origin)).CallBase().Verifiable();
      _classBuilderMock.Setup(mock => mock.AddMixin(typeof(BT2Mixin1), origin)).Returns(_mixinBuilderMock.Object).Verifiable();

      Assert.That(_classBuilderMock.Object.AddMixin<BT2Mixin1>(origin), Is.SameAs(_mixinBuilderMock.Object));
      _classBuilderMock.Verify();
    }

    [Test]
    [MethodImpl(MethodImplOptions.NoInlining)]
    public void AddMixin_Generic_WithoutOrigin ()
    {
      var expectedOrigin = MixinContextOrigin.CreateForMethod(MethodBase.GetCurrentMethod());

      _classBuilderMock.Setup(mock => mock.AddMixin<BT2Mixin1>(expectedOrigin)).Returns(_mixinBuilderMock.Object).Verifiable();

      Assert.That(_classBuilderMock.Object.AddMixin<BT2Mixin1>(), Is.SameAs(_mixinBuilderMock.Object));
      _classBuilderMock.Verify();
    }

    [Test]
    public void AddMixins_NonGeneric ()
    {
      var origin = MixinContextOriginObjectMother.Create();

      _classBuilderMock.Setup(mock => mock.AddMixins(origin, typeof(BT2Mixin1), typeof(BT3Mixin1), typeof(BT3Mixin2)))
          .CallBase()
          .Verifiable();
      _classBuilderMock.Setup(mock => mock.AddMixin(typeof(BT2Mixin1), origin)).Returns(_mixinBuilderMock.Object).Verifiable();
      _classBuilderMock.Setup(mock => mock.AddMixin(typeof(BT3Mixin1), origin)).Returns(_mixinBuilderMock.Object).Verifiable();
      _classBuilderMock.Setup(mock => mock.AddMixin(typeof(BT3Mixin2), origin)).Returns(_mixinBuilderMock.Object).Verifiable();

      var result = _classBuilderMock.Object.AddMixins(origin, typeof(BT2Mixin1), typeof(BT3Mixin1), typeof(BT3Mixin2));
      _classBuilderMock.Verify();

      Assert.That(result, Is.SameAs(_classBuilderMock.Object));
    }

    [Test]
    [MethodImpl(MethodImplOptions.NoInlining)]
    public void AddMixins_NonGeneric_WithoutOrigin ()
    {
      var expectedOrigin = MixinContextOrigin.CreateForMethod(MethodBase.GetCurrentMethod());

      _classBuilderMock.Setup(mock => mock.AddMixins(expectedOrigin, typeof(BT2Mixin1), typeof(BT3Mixin1), typeof(BT3Mixin2))).Returns(_classBuilderMock.Object).Verifiable();

      var result = _classBuilderMock.Object.AddMixins(typeof(BT2Mixin1), typeof(BT3Mixin1), typeof(BT3Mixin2));
      _classBuilderMock.Verify();

      Assert.That(result, Is.SameAs(_classBuilderMock.Object));
    }

    [Test]
    public void AddMixins_Generic2 ()
    {
      var origin = MixinContextOriginObjectMother.Create();

      _classBuilderMock.Setup(mock => mock.AddMixins<BT2Mixin1, BT3Mixin1>(origin))
           .CallBase()
           .Verifiable();
      _classBuilderMock.Setup(mock => mock.AddMixins(origin, typeof(BT2Mixin1), typeof(BT3Mixin1))).Returns(_classBuilderMock.Object).Verifiable();

      Assert.That(_classBuilderMock.Object.AddMixins<BT2Mixin1, BT3Mixin1>(origin), Is.SameAs(_classBuilderMock.Object));
      _classBuilderMock.Verify();
    }

    [Test]
    [MethodImpl(MethodImplOptions.NoInlining)]
    public void AddMixins_Generic2_WithoutOrigin ()
    {
      var expectedOrigin = MixinContextOrigin.CreateForMethod(MethodBase.GetCurrentMethod());

      _classBuilderMock.Setup(mock => mock.AddMixins<BT2Mixin1, BT3Mixin1>(expectedOrigin)).Returns(_classBuilderMock.Object).Verifiable();

      Assert.That(_classBuilderMock.Object.AddMixins<BT2Mixin1, BT3Mixin1>(), Is.SameAs(_classBuilderMock.Object));
      _classBuilderMock.Verify();
    }

    [Test]
    public void AddMixins_Generic3 ()
    {
      var origin = MixinContextOriginObjectMother.Create();

      _classBuilderMock
          .Setup(mock => mock.AddMixins<BT2Mixin1, BT3Mixin1, BT3Mixin2>(origin))
          .CallBase()
          .Verifiable();
      _classBuilderMock.Setup(mock => mock.AddMixins(origin, typeof(BT2Mixin1), typeof(BT3Mixin1), typeof(BT3Mixin2))).Returns(_classBuilderMock.Object).Verifiable();

      Assert.That(_classBuilderMock.Object.AddMixins<BT2Mixin1, BT3Mixin1, BT3Mixin2>(origin), Is.SameAs(_classBuilderMock.Object));
      _classBuilderMock.Verify();
    }

    [Test]
    [MethodImpl(MethodImplOptions.NoInlining)]
    public void AddMixins_Generic3_WithoutOrigin ()
    {
      var expectedOrigin = MixinContextOrigin.CreateForMethod(MethodBase.GetCurrentMethod());

      _classBuilderMock.Setup(mock => mock.AddMixins<BT2Mixin1, BT3Mixin1, BT3Mixin2>(expectedOrigin)).Returns(_classBuilderMock.Object).Verifiable();

      Assert.That(_classBuilderMock.Object.AddMixins<BT2Mixin1, BT3Mixin1, BT3Mixin2>(), Is.SameAs(_classBuilderMock.Object));
      _classBuilderMock.Verify();
    }

    [Test]
    public void AddOrderedMixins_NonGeneric ()
    {
      var origin = MixinContextOriginObjectMother.Create();

      Assert.That(_classBuilder.AddOrderedMixins(origin, typeof(BT2Mixin1), typeof(BT3Mixin1), typeof(BT3Mixin2)), Is.SameAs(_classBuilder));

      var mixinBuilders = new List<MixinContextBuilder>(_classBuilder.MixinContextBuilders);
      Assert.That(mixinBuilders.Count, Is.EqualTo(3));
      Assert.That(mixinBuilders[0].MixinType, Is.SameAs(typeof(BT2Mixin1)));
      Assert.That(mixinBuilders[0].Origin, Is.SameAs(origin));
      Assert.That(mixinBuilders[0].Dependencies, Is.Empty);
      Assert.That(mixinBuilders[1].MixinType, Is.SameAs(typeof(BT3Mixin1)));
      Assert.That(mixinBuilders[1].Origin, Is.SameAs(origin));
      Assert.That(mixinBuilders[1].Dependencies, Is.EquivalentTo(new object[] { typeof(BT2Mixin1) }));
      Assert.That(mixinBuilders[2].MixinType, Is.SameAs(typeof(BT3Mixin2)));
      Assert.That(mixinBuilders[2].Origin, Is.SameAs(origin));
      Assert.That(mixinBuilders[2].Dependencies, Is.EquivalentTo(new object[] { typeof(BT3Mixin1) }));
    }

    [Test]
    [MethodImpl(MethodImplOptions.NoInlining)]
    public void AddOrderedMixins_NonGeneric_WithoutOrigin ()
    {
      var expectedOrigin = MixinContextOrigin.CreateForMethod(MethodBase.GetCurrentMethod());

      _classBuilderMock.Setup(mock => mock.AddOrderedMixins(expectedOrigin, typeof(BT2Mixin1), typeof(BT3Mixin1), typeof(BT3Mixin2))).Returns(_classBuilderMock.Object).Verifiable();

      var result = _classBuilderMock.Object.AddOrderedMixins(typeof(BT2Mixin1), typeof(BT3Mixin1), typeof(BT3Mixin2));

      _parentBuilderMock.Verify();
      _classBuilderMock.Verify();
      _mixinBuilderMock.Verify();
      Assert.That(result, Is.SameAs(_classBuilderMock.Object));
    }

    [Test]
    public void AddOrderedMixins_Generic2 ()
    {
      var origin = MixinContextOriginObjectMother.Create();

      Assert.That(_classBuilder.AddOrderedMixins<BT2Mixin1, BT3Mixin1>(origin), Is.SameAs(_classBuilder));

      var mixinBuilders = new List<MixinContextBuilder>(_classBuilder.MixinContextBuilders);
      Assert.That(mixinBuilders.Count, Is.EqualTo(2));
      Assert.That(mixinBuilders[0].MixinType, Is.SameAs(typeof(BT2Mixin1)));
      Assert.That(mixinBuilders[0].Origin, Is.SameAs(origin));
      Assert.That(mixinBuilders[0].Dependencies, Is.Empty);
      Assert.That(mixinBuilders[1].MixinType, Is.SameAs(typeof(BT3Mixin1)));
      Assert.That(mixinBuilders[1].Origin, Is.SameAs(origin));
      Assert.That(mixinBuilders[1].Dependencies, Is.EquivalentTo(new object[] { typeof(BT2Mixin1) }));
    }

    [Test]
    [MethodImpl(MethodImplOptions.NoInlining)]
    public void AddOrderedMixins_Generic2_WithoutOrigin ()
    {
      var expectedOrigin = MixinContextOrigin.CreateForMethod(MethodBase.GetCurrentMethod());

      _classBuilderMock.Setup(mock => mock.AddOrderedMixins <BT2Mixin1, BT3Mixin1>(expectedOrigin)).Returns(_classBuilderMock.Object).Verifiable();

      var result = _classBuilderMock.Object.AddOrderedMixins<BT2Mixin1, BT3Mixin1>();

      _parentBuilderMock.Verify();
      _classBuilderMock.Verify();
      _mixinBuilderMock.Verify();
      Assert.That(result, Is.SameAs(_classBuilderMock.Object));
    }

    [Test]
    public void AddOrderedMixins_Generic3 ()
    {
      var origin = MixinContextOriginObjectMother.Create();

      Assert.That(_classBuilder.AddOrderedMixins<BT2Mixin1, BT3Mixin1, BT3Mixin2>(origin), Is.SameAs(_classBuilder));

      var mixinBuilders = new List<MixinContextBuilder>(_classBuilder.MixinContextBuilders);
      Assert.That(mixinBuilders.Count, Is.EqualTo(3));
      Assert.That(mixinBuilders[0].MixinType, Is.SameAs(typeof(BT2Mixin1)));
      Assert.That(mixinBuilders[0].Origin, Is.SameAs(origin));
      Assert.That(mixinBuilders[0].Dependencies, Is.Empty);
      Assert.That(mixinBuilders[1].MixinType, Is.SameAs(typeof(BT3Mixin1)));
      Assert.That(mixinBuilders[1].Origin, Is.SameAs(origin));
      Assert.That(mixinBuilders[1].Dependencies, Is.EquivalentTo(new object[] { typeof(BT2Mixin1) }));
      Assert.That(mixinBuilders[2].MixinType, Is.SameAs(typeof(BT3Mixin2)));
      Assert.That(mixinBuilders[2].Origin, Is.SameAs(origin));
      Assert.That(mixinBuilders[2].Dependencies, Is.EquivalentTo(new object[] { typeof(BT3Mixin1) }));
    }

    [Test]
    [MethodImpl(MethodImplOptions.NoInlining)]
    public void AddOrderedMixins_Generic3_WithoutOrigin ()
    {
      var expectedOrigin = MixinContextOrigin.CreateForMethod(MethodBase.GetCurrentMethod());

      _classBuilderMock.Setup(mock => mock.AddOrderedMixins<BT2Mixin1, BT3Mixin1, BT3Mixin2>(expectedOrigin)).Returns(_classBuilderMock.Object).Verifiable();

      var result = _classBuilderMock.Object.AddOrderedMixins<BT2Mixin1, BT3Mixin1, BT3Mixin2>();

      _parentBuilderMock.Verify();
      _classBuilderMock.Verify();
      _mixinBuilderMock.Verify();
      Assert.That(result, Is.SameAs(_classBuilderMock.Object));
    }

    [Test]
    public void EnsureMixin_NonGeneric ()
    {
      var origin = MixinContextOriginObjectMother.Create();

      MixinContextBuilder builder = _classBuilder.EnsureMixin(typeof(BT2Mixin1), origin);

      Assert.That(builder.MixinType, Is.EqualTo(typeof(BT2Mixin1)));
      Assert.That(builder.Origin, Is.EqualTo(origin));
      Type[] mixinTypes = GetMixinTypes();
      Assert.That(mixinTypes, Is.EquivalentTo(new object[] { typeof(BT2Mixin1) }));

      var otherOrigin = MixinContextOriginObjectMother.Create("some other kind");
      Assert.That(_classBuilder.EnsureMixin(typeof(BT2Mixin1), otherOrigin), Is.SameAs(builder));
      Type[] mixinTypesAfterSecondEnsureMixin = GetMixinTypes();
      Assert.That(mixinTypesAfterSecondEnsureMixin, Is.EquivalentTo(new object[] { typeof(BT2Mixin1) }));
      Assert.That(builder.Origin, Is.EqualTo(origin));
    }

    [Test]
    [MethodImpl(MethodImplOptions.NoInlining)]
    public void EnsureMixin_NonGeneric_WithoutOrigin ()
    {
      var expectedOrigin = MixinContextOrigin.CreateForMethod(MethodBase.GetCurrentMethod());

      _classBuilderMock.Setup(mock => mock.EnsureMixin(typeof(BT2Mixin1), expectedOrigin)).Returns(_mixinBuilderMock.Object).Verifiable();

      var result = _classBuilderMock.Object.EnsureMixin(typeof(BT2Mixin1));

      _parentBuilderMock.Verify();
      _classBuilderMock.Verify();
      _mixinBuilderMock.Verify();
      Assert.That(result, Is.SameAs(_mixinBuilderMock.Object));
    }

    [Test]
    public void EnsureMixin_Inheritance ()
    {
      var contextWithMixin = ClassContextObjectMother.Create(typeof(BaseType3), typeof(NullTarget));

      MixinContextBuilder builder = _classBuilder.EnsureMixin(typeof(DerivedNullTarget));
      Assert.That(builder.MixinType, Is.EqualTo(typeof(DerivedNullTarget)));
      Type[] mixinTypes = GetMixinTypes();
      Assert.That(mixinTypes, Is.EquivalentTo(new object[] { typeof(DerivedNullTarget) }));

      ClassContext builtContext = _classBuilder.BuildClassContext(new[] {contextWithMixin});
      Assert.That(builtContext.Mixins.Count, Is.EqualTo(1));
      Assert.That(builtContext.Mixins.ContainsKey(typeof(DerivedNullTarget)), Is.True);
      Assert.That(builtContext.Mixins.ContainsKey(typeof(NullTarget)), Is.False);
    }

    [Test]
    public void EnsureMixin_Generic ()
    {
      var origin = MixinContextOriginObjectMother.Create();

      _classBuilderMock.Setup(mock => mock.EnsureMixin<BT2Mixin1>(origin)).CallBase().Verifiable();
      _classBuilderMock.Setup(mock => mock.EnsureMixin(typeof(BT2Mixin1), origin)).Returns(_mixinBuilderMock.Object).Verifiable();

      Assert.That(_classBuilderMock.Object.EnsureMixin<BT2Mixin1>(origin), Is.SameAs(_mixinBuilderMock.Object));
      _classBuilderMock.Verify();
    }

    [Test]
    [MethodImpl(MethodImplOptions.NoInlining)]
    public void EnsureMixin_Generic_WithoutOrigin ()
    {
      var expectedOrigin = MixinContextOrigin.CreateForMethod(MethodBase.GetCurrentMethod());

      _classBuilderMock.Setup(mock => mock.EnsureMixin<BT2Mixin1>(expectedOrigin)).Returns(_mixinBuilderMock.Object).Verifiable();

      Assert.That(_classBuilderMock.Object.EnsureMixin<BT2Mixin1>(), Is.SameAs(_mixinBuilderMock.Object));
      _classBuilderMock.Verify();
    }

    [Test]
    public void EnsureMixins_NonGeneric ()
    {
      var origin = MixinContextOriginObjectMother.Create();

      _classBuilderMock
          .Setup(mock => mock.EnsureMixins(origin, typeof(BT2Mixin1), typeof(BT3Mixin1), typeof(BT3Mixin2)))
          .CallBase()
          .Verifiable();
      _classBuilderMock.Setup(mock => mock.EnsureMixin(typeof(BT2Mixin1), origin)).Returns(_mixinBuilderMock.Object).Verifiable();
      _classBuilderMock.Setup(mock => mock.EnsureMixin(typeof(BT3Mixin1), origin)).Returns(_mixinBuilderMock.Object).Verifiable();
      _classBuilderMock.Setup(mock => mock.EnsureMixin(typeof(BT3Mixin2), origin)).Returns(_mixinBuilderMock.Object).Verifiable();

      Assert.That(_classBuilderMock.Object.EnsureMixins(origin, typeof(BT2Mixin1), typeof(BT3Mixin1), typeof(BT3Mixin2)), Is.SameAs(_classBuilderMock.Object));
      _classBuilderMock.Verify();
    }

    [Test]
    [MethodImpl(MethodImplOptions.NoInlining)]
    public void EnsureMixins_NonGeneric_WithoutOrigin ()
    {
      var expectedOrigin = MixinContextOrigin.CreateForMethod(MethodBase.GetCurrentMethod());

      _classBuilderMock
          .Setup(mock => mock.EnsureMixins(expectedOrigin, typeof(BT2Mixin1), typeof(BT3Mixin1), typeof(BT3Mixin2)))
          .Returns(_classBuilderMock.Object)
          .Verifiable();

      Assert.That(_classBuilderMock.Object.EnsureMixins(typeof(BT2Mixin1), typeof(BT3Mixin1), typeof(BT3Mixin2)), Is.SameAs(_classBuilderMock.Object));
      _classBuilderMock.Verify();
    }

    [Test]
    public void EnsureMixins_Generic2 ()
    {
      var origin = MixinContextOriginObjectMother.Create();

      _classBuilderMock.Setup(mock => mock.EnsureMixins<BT2Mixin1, BT3Mixin1>(origin))
           .CallBase()
           .Verifiable();
      _classBuilderMock.Setup(mock => mock.EnsureMixins(origin, typeof(BT2Mixin1), typeof(BT3Mixin1))).Returns(_classBuilderMock.Object).Verifiable();

      Assert.That(_classBuilderMock.Object.EnsureMixins<BT2Mixin1, BT3Mixin1>(origin), Is.SameAs(_classBuilderMock.Object));
      _classBuilderMock.Verify();
    }

    [Test]
    [MethodImpl(MethodImplOptions.NoInlining)]
    public void EnsureMixins_Generic2_WithoutOrigin ()
    {
      var expectedOrigin = MixinContextOrigin.CreateForMethod(MethodBase.GetCurrentMethod());

      _classBuilderMock.Setup(mock => mock.EnsureMixins<BT2Mixin1, BT3Mixin1>(expectedOrigin)).Returns(_classBuilderMock.Object).Verifiable();

      Assert.That(_classBuilderMock.Object.EnsureMixins<BT2Mixin1, BT3Mixin1>(), Is.SameAs(_classBuilderMock.Object));
      _classBuilderMock.Verify();
    }

    [Test]
    public void EnsureMixins_Generic3 ()
    {
      var origin = MixinContextOriginObjectMother.Create();

      _classBuilderMock.Setup(mock => mock.EnsureMixins<BT2Mixin1, BT3Mixin1, BT3Mixin2>(origin))
          .CallBase()
          .Verifiable();
      _classBuilderMock.Setup(mock => mock.EnsureMixins(origin, typeof(BT2Mixin1), typeof(BT3Mixin1), typeof(BT3Mixin2))).Returns(_classBuilderMock.Object).Verifiable();

      Assert.That(_classBuilderMock.Object.EnsureMixins<BT2Mixin1, BT3Mixin1, BT3Mixin2>(origin), Is.SameAs(_classBuilderMock.Object));
      _classBuilderMock.Verify();
    }

    [Test]
    [MethodImpl(MethodImplOptions.NoInlining)]
    public void EnsureMixins_Generic3_WithoutOrigin ()
    {
      var expectedOrigin = MixinContextOrigin.CreateForMethod(MethodBase.GetCurrentMethod());

      _classBuilderMock.Setup(mock => mock.EnsureMixins<BT2Mixin1, BT3Mixin1, BT3Mixin2>(expectedOrigin)).Returns(_classBuilderMock.Object).Verifiable();

      Assert.That(_classBuilderMock.Object.EnsureMixins<BT2Mixin1, BT3Mixin1, BT3Mixin2>(), Is.SameAs(_classBuilderMock.Object));
      _classBuilderMock.Verify();
    }

    [Test]
    public void AddComposedInterface_NonGeneric ()
    {
      Assert.That(_classBuilder.AddComposedInterface(typeof(IBT6Mixin1)), Is.SameAs(_classBuilder));
      Assert.That(_classBuilder.ComposedInterfaces.ToArray(), Is.EquivalentTo(new object[] { typeof(IBT6Mixin1) }));
    }

    [Test]
    public void AddComposedInterface_Twice ()
    {
      Assert.That(
          () => _classBuilder.AddComposedInterface(typeof(IBT6Mixin1)).AddComposedInterface(typeof(IBT6Mixin1)),
          Throws.ArgumentException
              .With.Message.Contains(
                  "Remotion.Mixins.UnitTests.Core.TestDomain.IBT6Mixin1 is already configured as a "
                  + "composed interface for type Remotion.Mixins.UnitTests.Core.TestDomain.BaseType2."));
    }

    [Test]
    public void AddComposedInterface_Generic ()
    {
      _classBuilderMock.Setup(mock => mock.AddComposedInterface<BT2Mixin1>()).CallBase().Verifiable();
      _classBuilderMock.Setup(mock => mock.AddComposedInterface(typeof(BT2Mixin1))).Returns(_classBuilderMock.Object).Verifiable();

      Assert.That(_classBuilderMock.Object.AddComposedInterface<BT2Mixin1>(), Is.SameAs(_classBuilderMock.Object));
      _classBuilderMock.Verify();
    }

    [Test]
    public void AddComposedInterfaces_NonGeneric ()
    {
      _classBuilderMock.Setup(mock => mock.AddComposedInterfaces(typeof(BT2Mixin1), typeof(BT3Mixin1), typeof(BT3Mixin2)))
          .CallBase()
          .Verifiable();
      _classBuilderMock.Setup(mock => mock.AddComposedInterface(typeof(BT2Mixin1))).Returns(_classBuilderMock.Object).Verifiable();
      _classBuilderMock.Setup(mock => mock.AddComposedInterface(typeof(BT3Mixin1))).Returns(_classBuilderMock.Object).Verifiable();
      _classBuilderMock.Setup(mock => mock.AddComposedInterface(typeof(BT3Mixin2))).Returns(_classBuilderMock.Object).Verifiable();

      Assert.That(_classBuilderMock.Object.AddComposedInterfaces(typeof(BT2Mixin1), typeof(BT3Mixin1), typeof(BT3Mixin2)), Is.SameAs(_classBuilderMock.Object));
      _classBuilderMock.Verify();
    }

    [Test]
    public void AddComposedInterfaces_Generic2 ()
    {
      _classBuilderMock.Setup(mock => mock.AddComposedInterfaces<BT2Mixin1, BT3Mixin1>())
           .CallBase()
           .Verifiable();
      _classBuilderMock.Setup(mock => mock.AddComposedInterfaces(typeof(BT2Mixin1), typeof(BT3Mixin1))).Returns(_classBuilderMock.Object).Verifiable();

      Assert.That(_classBuilderMock.Object.AddComposedInterfaces<BT2Mixin1, BT3Mixin1>(), Is.SameAs(_classBuilderMock.Object));
      _classBuilderMock.Verify();
    }

    [Test]
    public void AddComposedInterfaces_Generic3 ()
    {
      _classBuilderMock.Setup(mock => mock.AddComposedInterfaces<BT2Mixin1, BT3Mixin1, BT3Mixin2>())
          .CallBase()
          .Verifiable();
      _classBuilderMock.Setup(mock => mock.AddComposedInterfaces(typeof(BT2Mixin1), typeof(BT3Mixin1), typeof(BT3Mixin2))).Returns(_classBuilderMock.Object).Verifiable();

      Assert.That(_classBuilderMock.Object.AddComposedInterfaces<BT2Mixin1, BT3Mixin1, BT3Mixin2>(), Is.SameAs(_classBuilderMock.Object));
      _classBuilderMock.Verify();
    }

    [Test]
    public void SuppressMixin_Rule ()
    {
      var ruleStub = new Mock<IMixinSuppressionRule>();

      Assert.That(_classBuilder.SuppressedMixins, Is.Empty);
      _classBuilder.SuppressMixin(ruleStub.Object);
      Assert.That(_classBuilder.SuppressedMixins, Is.EquivalentTo(new[] { ruleStub.Object }));
    }

    [Test]
    public void SuppressMixin_NonGeneric ()
    {
      Assert.That(_classBuilder.SuppressedMixins, Is.Empty);
      _classBuilder.SuppressMixin(typeof(BT1Mixin1));
      _classBuilder.SuppressMixin(typeof(BT2Mixin1));
      Assert.That(
          _classBuilder.SuppressedMixins.Cast<MixinTreeSuppressionRule>().Select(r => r.MixinBaseTypeToSuppress).ToArray(),
          Is.EquivalentTo(new object[] { typeof(BT2Mixin1), typeof(BT1Mixin1) }));
    }

    [Test]
    public void SuppressMixin_Generic ()
    {
      _classBuilderMock.Setup(mock => mock.SuppressMixin<BT2Mixin1>()).CallBase().Verifiable();
      _classBuilderMock.Setup(mock => mock.SuppressMixin(typeof(BT2Mixin1))).Returns(_classBuilderMock.Object).Verifiable();

      Assert.That(_classBuilderMock.Object.SuppressMixin<BT2Mixin1>(), Is.SameAs(_classBuilderMock.Object));
      _classBuilderMock.Verify();
    }

    [Test]
    public void SuppressMixins_NonGeneric ()
    {
      _classBuilderMock.Setup(mock => mock.SuppressMixins(typeof(BT2Mixin1), typeof(BT3Mixin1), typeof(BT3Mixin2)))
          .CallBase()
          .Verifiable();
      _classBuilderMock.Setup(mock => mock.SuppressMixin(typeof(BT2Mixin1))).Returns(_classBuilderMock.Object).Verifiable();
      _classBuilderMock.Setup(mock => mock.SuppressMixin(typeof(BT3Mixin1))).Returns(_classBuilderMock.Object).Verifiable();
      _classBuilderMock.Setup(mock => mock.SuppressMixin(typeof(BT3Mixin2))).Returns(_classBuilderMock.Object).Verifiable();

      Assert.That(_classBuilderMock.Object.SuppressMixins(typeof(BT2Mixin1), typeof(BT3Mixin1), typeof(BT3Mixin2)), Is.SameAs(_classBuilderMock.Object));
      _classBuilderMock.Verify();
    }

    [Test]
    public void SuppressMixins_Generic2 ()
    {
      _classBuilderMock.Setup(mock => mock.SuppressMixins<BT2Mixin1, BT3Mixin1>())
           .CallBase()
           .Verifiable();
      _classBuilderMock.Setup(mock => mock.SuppressMixins(typeof(BT2Mixin1), typeof(BT3Mixin1))).Returns(_classBuilderMock.Object).Verifiable();

      Assert.That(_classBuilderMock.Object.SuppressMixins<BT2Mixin1, BT3Mixin1>(), Is.SameAs(_classBuilderMock.Object));
      _classBuilderMock.Verify();
    }

    [Test]
    public void SuppressMixins_Generic3 ()
    {
      _classBuilderMock.Setup(mock => mock.SuppressMixins<BT2Mixin1, BT3Mixin1, BT3Mixin2>())
          .CallBase()
          .Verifiable();
      _classBuilderMock.Setup(mock => mock.SuppressMixins(typeof(BT2Mixin1), typeof(BT3Mixin1), typeof(BT3Mixin2))).Returns(_classBuilderMock.Object).Verifiable();

      Assert.That(_classBuilderMock.Object.SuppressMixins<BT2Mixin1, BT3Mixin1, BT3Mixin2>(), Is.SameAs(_classBuilderMock.Object));
      _classBuilderMock.Verify();
    }

    [Test]
    public void AddMixinDependency_NonGeneric ()
    {
      var result = _classBuilder.AddMixinDependency(typeof(BT1Mixin1), typeof(int));
      Assert.That(result, Is.SameAs(_classBuilder));

      _classBuilder.AddMixinDependency(typeof(BT1Mixin1), typeof(float));
      _classBuilder.AddMixinDependency(typeof(BT1Mixin2), typeof(double));

      Assert.That(_classBuilder.MixinDependencies.Count(), Is.EqualTo(2));

      var mixinDependencySpecification1 = _classBuilder.MixinDependencies.Single(dep => dep.MixinType == typeof(BT1Mixin1));
      Assert.That(mixinDependencySpecification1.Dependencies, Is.EqualTo(new[] { typeof(int), typeof(float) }));

      var mixinDependencySpecification2 = _classBuilder.MixinDependencies.Single(dep => dep.MixinType == typeof(BT1Mixin2));
      Assert.That(mixinDependencySpecification2.Dependencies, Is.EqualTo(new[] { typeof(double) }));
    }

    [Test]
    public void AddMixinDependency_Generic ()
    {
      _classBuilderMock.Setup(mock => mock.AddMixinDependency<BT2Mixin1, int>()).CallBase().Verifiable();
      _classBuilderMock.Setup(mock => mock.AddMixinDependency(typeof(BT2Mixin1), typeof(int))).Returns(_classBuilderMock.Object).Verifiable();

      Assert.That(_classBuilderMock.Object.AddMixinDependency<BT2Mixin1, int>(), Is.SameAs(_classBuilderMock.Object));
      _classBuilderMock.Verify();
    }

    [Test]
    public void BuildContext_NoInheritance ()
    {
      _classBuilder.AddMixins<BT1Mixin1, BT1Mixin2>();
      _classBuilder.AddComposedInterfaces<IBT6Mixin1, IBT6Mixin2>();

      ClassContext builtContext = _classBuilder.BuildClassContext();

      Assert.That(builtContext.Mixins.Count, Is.EqualTo(2));
      Assert.That(builtContext.Mixins.ContainsKey(typeof(BT1Mixin1)), Is.True);
      Assert.That(builtContext.Mixins.ContainsKey(typeof(BT1Mixin2)), Is.True);

      Assert.That(builtContext.ComposedInterfaces.Count, Is.EqualTo(2));
      Assert.That(builtContext.ComposedInterfaces, Has.Member(typeof(IBT6Mixin1)));
      Assert.That(builtContext.ComposedInterfaces, Has.Member(typeof(IBT6Mixin2)));
    }

    [Test]
    public void BuildContext_SuppressedInheritance ()
    {
      ClassContext inheritedContext = new ClassContextBuilder(typeof(BaseType2))
          .AddMixin(typeof(BT3Mixin1))
          .AddComposedInterface(typeof(BT1Mixin2))
          .BuildClassContext();

      _classBuilder.Clear();
      _classBuilder.AddMixins<BT1Mixin1, BT1Mixin2>();
      _classBuilder.AddComposedInterfaces<IBT6Mixin1, IBT6Mixin2>();

      ClassContext builtContext = _classBuilder.BuildClassContext(new[] { inheritedContext });

      Assert.That(builtContext.Mixins.Count, Is.EqualTo(2));
      Assert.That(builtContext.Mixins.ContainsKey(typeof(BT1Mixin1)), Is.True);
      Assert.That(builtContext.Mixins.ContainsKey(typeof(BT1Mixin2)), Is.True);

      Assert.That(builtContext.ComposedInterfaces.Count, Is.EqualTo(2));
      Assert.That(builtContext.ComposedInterfaces, Has.Member(typeof(IBT6Mixin1)));
      Assert.That(builtContext.ComposedInterfaces, Has.Member(typeof(IBT6Mixin2)));
    }

    [Test]
    public void BuildContext_WithInheritance ()
    {
      ClassContext inheritedContext = new ClassContextBuilder(typeof(BaseType7))
          .AddMixin(typeof(BT7Mixin1))
          .AddComposedInterface(typeof(BT1Mixin2))
          .BuildClassContext();

      _classBuilder.AddMixins<BT1Mixin1, BT1Mixin2>();
      _classBuilder.AddComposedInterfaces<IBT6Mixin1, IBT6Mixin2>();

      ClassContext builtContext = _classBuilder.BuildClassContext(new[] { inheritedContext });

      Assert.That(builtContext.Mixins.Count, Is.EqualTo(3));
      Assert.That(builtContext.Mixins.ContainsKey(typeof(BT1Mixin1)), Is.True);
      Assert.That(builtContext.Mixins.ContainsKey(typeof(BT1Mixin2)), Is.True);
      Assert.That(builtContext.Mixins.ContainsKey(typeof(BT7Mixin1)), Is.True);

      Assert.That(builtContext.ComposedInterfaces.Count, Is.EqualTo(3));
      Assert.That(builtContext.ComposedInterfaces, Has.Member(typeof(IBT6Mixin1)));
      Assert.That(builtContext.ComposedInterfaces, Has.Member(typeof(IBT6Mixin2)));
      Assert.That(builtContext.ComposedInterfaces, Has.Member(typeof(BT1Mixin2)));
    }

    [Test]
    public void BuildContext_ExtendParentContext ()
    {
      var classContextBuilder = new ClassContextBuilder(_parentBuilderMock.Object, typeof(BaseType2));
      classContextBuilder.AddMixins<BT1Mixin1, BT1Mixin2>();

      var parentContext = ClassContextObjectMother.Create(typeof(BaseType2), typeof(BT2Mixin1));
      ClassContext builtContext = classContextBuilder.BuildClassContext(new[] { parentContext });

      Assert.That(builtContext.Mixins.Count, Is.EqualTo(3));
      Assert.That(builtContext.Mixins.ContainsKey(typeof(BT2Mixin1)), Is.True);
      Assert.That(builtContext.Mixins.ContainsKey(typeof(BT1Mixin1)), Is.True);
      Assert.That(builtContext.Mixins.ContainsKey(typeof(BT1Mixin2)), Is.True);
    }

    [Test]
    public void BuildContext_ReplaceParentContext ()
    {
      var classContextBuilder = new ClassContextBuilder(_parentBuilderMock.Object, typeof(BaseType2));
      classContextBuilder.Clear().AddMixins<BT1Mixin1, BT1Mixin2>();

      var parentContext = ClassContextObjectMother.Create(typeof(BaseType2), typeof(BT2Mixin1));
      ClassContext builtContext = classContextBuilder.BuildClassContext(new[] { parentContext });

      Assert.That(builtContext.Mixins.Count, Is.EqualTo(2));
      Assert.That(builtContext.Mixins.ContainsKey(typeof(BT1Mixin1)), Is.True);
      Assert.That(builtContext.Mixins.ContainsKey(typeof(BT1Mixin2)), Is.True);
    }

    [Test]
    public void BuildContext_Suppression ()
    {
      var classContextBuilder = new ClassContextBuilder(_parentBuilderMock.Object, typeof(BaseType2));
      classContextBuilder.AddMixins<BT1Mixin1, BT1Mixin2>();

      classContextBuilder.SuppressMixins(typeof(IBT1Mixin1), typeof(BT5Mixin1), typeof(BT3Mixin3<,>));

      var inheritedContext = ClassContextObjectMother.Create(typeof(BaseType2), typeof(BT3Mixin1), typeof(BT3Mixin3<IBaseType33, IBaseType33>));
      var parentContext = ClassContextObjectMother.Create(typeof(BaseType2), typeof(BT5Mixin1), typeof(BT5Mixin2));
      ClassContext builtContext = classContextBuilder.BuildClassContext(new[] { inheritedContext, parentContext });

      Assert.That(builtContext.Mixins.Count, Is.EqualTo(3));
      Assert.That(builtContext.Mixins.ContainsKey(typeof(BT3Mixin1)), Is.True);
      Assert.That(builtContext.Mixins.ContainsKey(typeof(BT5Mixin2)), Is.True);
      Assert.That(builtContext.Mixins.ContainsKey(typeof(BT1Mixin2)), Is.True);
    }

    [Test]
    public void BuildContext_MixinDependencies ()
    {
      _classBuilder
          .AddMixinDependency<BT1Mixin1, BT1Mixin2>()
          .AddMixin<BT1Mixin1>();

      var builtContext = _classBuilder.BuildClassContext();

      Assert.That(builtContext.Mixins[typeof(BT1Mixin1)].ExplicitDependencies, Has.Member(typeof(BT1Mixin2)));
    }

    [Test]
    public void BuildContext_MixinDependencies_AppliedWithInheritance ()
    {
      _classBuilder.AddMixinDependency<BT1Mixin1, BT1Mixin2>();
      _classBuilder.AddMixinDependency<BT1Mixin2, BT2Mixin1>();

      var inheritedContext1 = ClassContextObjectMother.Create(typeof(NullTarget), typeof(BT1Mixin1));
      var inheritedContext2 = ClassContextObjectMother.Create(typeof(NullTarget), typeof(BT1Mixin2));
      var builtContext = _classBuilder.BuildClassContext(new[] { inheritedContext1, inheritedContext2 });

      Assert.That(builtContext.Mixins[typeof(BT1Mixin1)].ExplicitDependencies, Has.Member(typeof(BT1Mixin2)));
      Assert.That(builtContext.Mixins[typeof(BT1Mixin2)].ExplicitDependencies, Has.Member(typeof(BT2Mixin1)));
    }

    [Test]
    public void BuildContext_MixinDependencies_Error ()
    {
      _classBuilder.AddMixinDependency<BT1Mixin1, BT1Mixin2>();

      Assert.That(
          () => _classBuilder.BuildClassContext(),
          Throws.TypeOf<ConfigurationException>().With.Message.EqualTo(
              "The mixin dependencies configured for type 'Remotion.Mixins.UnitTests.Core.TestDomain.BaseType2' could not be processed: "
              + "The mixin 'Remotion.Mixins.UnitTests.Core.TestDomain.BT1Mixin1' is not configured for class "
              + "'Remotion.Mixins.UnitTests.Core.TestDomain.BaseType2'."));
    }

    [Test]
    public void BuildContext_MixinDependencies_AppliedAfterSuppression ()
    {
      _classBuilder
          .AddMixinDependency<BT1Mixin1, BT1Mixin2>()
          .AddMixin<BT1Mixin1>()
          .SuppressMixin<BT1Mixin1>();

      Assert.That(
          () => _classBuilder.BuildClassContext(),
          Throws.TypeOf<ConfigurationException>().With.Message.EqualTo(
              "The mixin dependencies configured for type 'Remotion.Mixins.UnitTests.Core.TestDomain.BaseType2' could not be processed: "
              + "The mixin 'Remotion.Mixins.UnitTests.Core.TestDomain.BT1Mixin1' is not configured for class "
              + "'Remotion.Mixins.UnitTests.Core.TestDomain.BaseType2'."));
    }

    [Test]
    public void ParentMembers ()
    {
      _mixinBuilderMock.Reset();
      _classBuilderMock.Reset();
      _parentBuilderMock.Reset();

      var r1 = new ClassContextBuilder(new MixinConfigurationBuilder(null), typeof(object));
      var r2 = new MixinConfiguration();
      var r3 = new Mock<IDisposable>(MockBehavior.Strict);

      var sequence = new MockSequence();
      _parentBuilderMock.InSequence(sequence).Setup(mock => mock.ForClass<object>()).Returns(r1).Verifiable();
      _parentBuilderMock.InSequence(sequence).Setup(mock => mock.ForClass<string>()).Returns(r1).Verifiable();
      _parentBuilderMock.InSequence(sequence).Setup(mock => mock.BuildConfiguration()).Returns(r2).Verifiable();
      _parentBuilderMock.InSequence(sequence).Setup(mock => mock.EnterScope()).Returns(r3.Object).Verifiable();


      Assert.That(_classBuilder.ForClass<object>(), Is.SameAs(r1));
      Assert.That(_classBuilder.ForClass<string>(), Is.SameAs(r1));
      Assert.That(_classBuilder.BuildConfiguration(), Is.SameAs(r2));
      Assert.That(_classBuilder.EnterScope(), Is.SameAs(r3.Object));

      _parentBuilderMock.Verify();
      _classBuilderMock.Verify();
      _mixinBuilderMock.Verify();
      r3.Verify();
    }
  }
}
