using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using ProjectCeilidh.Cobble.Generator;
using Xunit;

namespace ProjectCeilidh.Cobble.Tests
{
    public class CobbleContextTests : IDisposable
    {
        private readonly CobbleContext _context;

        public CobbleContextTests()
        {
            _context = new CobbleContext();
        }

        [Fact]
        public void BasicLoad()
        {
            _context.AddManaged<TestUnit>();
            _context.Execute();

            Assert.True(_context.TryGetSingleton<TestUnit>(out _));
            Assert.True(_context.TryGetSingleton<ITestUnit>(out _));
        }

        [Fact]
        public void BasicDeps()
        {
            _context.AddManaged<TestUnit>();
            _context.AddManaged<BasicDependUnit>();
            _context.Execute();

            Assert.True(_context.TryGetSingleton<TestUnit>(out _));
            Assert.True(_context.TryGetSingleton<ITestUnit>(out _));
            Assert.True(_context.TryGetSingleton<BasicDependUnit>(out _));
        }

        [Fact]
        public void MediumDeps()
        {
            _context.AddManaged<TestUnit>();
            _context.AddManaged<MediumDependUnit>();
            _context.Execute();

            Assert.True(_context.TryGetSingleton<TestUnit>(out _));
            Assert.True(_context.TryGetSingleton<ITestUnit>(out _));
            Assert.True(_context.TryGetSingleton<MediumDependUnit>(out _));
        }

        [Fact]
        public void AdvancedDeps()
        {
            _context.AddManaged<TestUnit>();
            _context.AddManaged<AdvancedDependUnit>();
            _context.Execute();

            _context.AddManaged<TestUnit>();

            Assert.True(_context.TryGetImplementations<TestUnit>(out var testSet) && testSet.Count() == 2);
            Assert.True(_context.TryGetSingleton<AdvancedDependUnit>(out var adv) && adv.TestUnits.Count == 2);
        }

        [Fact]
        public void DuplicateResolver()
        {
            var exec = false;

            _context.DuplicateResolver = (dependencyType, instances) => {
                exec = true;
                return instances[0];
            };

            _context.AddManaged<TestUnit>();
            _context.AddManaged<TestUnit>();
            _context.AddManaged<BasicDependUnit>();

            _context.Execute();

            Assert.True(exec);
        }

        [Fact]
        public void DuplicateException()
        {
            _context.AddManaged<TestUnit>();
            _context.AddManaged<TestUnit>();
            _context.AddManaged<BasicDependUnit>();

            Assert.Throws<AmbiguousDependencyException>(() => _context.Execute());
        }

        [Fact]
        public void DictInstanceGenerator()
        {
            _context.AddManaged<TestUnit>();
            _context.AddManaged(new DictionaryInstanceGenerator(typeof(ITestUnit), new Func<TestUnit, object>(x => x), new Dictionary<MethodInfo, Delegate>
            {
                [typeof(ITestUnit).GetMethod("get_TestValue")] = new Func<TestUnit, string>(x => x.TestValue)
            }));

            _context.Execute();
            Assert.True(_context.TryGetImplementations<ITestUnit>(out var units));
            foreach (var testUnit in units)
                Assert.Equal("Hi", testUnit.TestValue);
        }

        [Fact]
        public void DisposeTest()
        {
            _context.AddManaged<DisposeTestUnit>();

            _context.Execute();
            Assert.True(_context.TryGetSingleton(out DisposeTestUnit testUnit) && !testUnit.IsDisposed);
            _context.Dispose();
            Assert.True(testUnit.IsDisposed);
        }

        private class DisposeTestUnit : IDisposable
        {
            public bool IsDisposed { get; private set; }

            public DisposeTestUnit()
            {
                IsDisposed = false;
            }

            public void Dispose()
            {
                IsDisposed = true;
            }
        }

        private interface ITestUnit
        {
            string TestValue { get; }
        }

        private class TestUnit : ITestUnit
        {
            public string TestValue => "Hi";
        }

        private class BasicDependUnit
        {
            public BasicDependUnit(TestUnit unit)
            {
                Assert.NotNull(unit);
                Assert.Equal("Hi", unit.TestValue);
            }
        }

        private class MediumDependUnit
        {
            public MediumDependUnit(IEnumerable<ITestUnit> units)
            {
                Assert.NotNull(units);
                Assert.NotEmpty(units);
            }
        }

        private class AdvancedDependUnit : ILateInject<ITestUnit>
        {
            public readonly List<ITestUnit> TestUnits;

            public AdvancedDependUnit(IEnumerable<ITestUnit> units)
            {
                TestUnits = new List<ITestUnit>(units);

                Assert.NotEmpty(TestUnits);
            }

            public void UnitLoaded(ITestUnit unit)
            {
                TestUnits.Add(unit);
            }
        }

        public void Dispose()
        {
            _context.Dispose();
        }
    }
}
