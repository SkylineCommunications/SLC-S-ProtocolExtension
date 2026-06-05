namespace Skyline.DataMiner.Utils.Protocol.Extension.Tests
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using Microsoft.VisualStudio.TestTools.UnitTesting;

    using NSubstitute;

    using Skyline.DataMiner.Scripting;

    [TestClass]
    public class ProtocolExtensionTests
    {
        private const int TableId = 1000;


        [TestMethod]
        public void GetColumns_T1T2TReturn_InvalidResult_ThrowsOnCallNotOnIteration_DemonstratesOldDeferredBehavior()
        {
            // Arrange
            var protocol = Substitute.For<SLProtocol>();
            var columnIndices = new uint[] { 0, 1 };

            // NotifyProtocol returns null, which ValidateResult considers invalid.
            protocol.NotifyProtocol(321, TableId, Arg.Any<object>()).Returns((object)null);

            // It is only when iteration is triggered that ValidateResult runs and throws.
            Assert.ThrowsException<InvalidOperationException>(() => protocol.GetColumns(
                TableId,
                columnIndices,
                (string col0, string col1) => $"{col0}-{col1}"));
        }

        [TestMethod]
        public void GetColumn_T_InvokesNotifyProtocol321WithCorrectTableIdAndColumnIdx()
        {
            var protocol = Substitute.For<SLProtocol>();
            uint colIdx = 2;
            protocol.NotifyProtocol(321, TableId, Arg.Is<object>(o => ((uint[])o).SequenceEqual(new[] { colIdx })))
                    .Returns(new object[] { new object[] { "10", "20", "30" } });

            var result = protocol.GetColumn<int>(TableId, colIdx).ToList();

            protocol.Received(1).NotifyProtocol(
                321,
                TableId,
                Arg.Is<object>(o => ((uint[])o).SequenceEqual(new[] { colIdx })));

            CollectionAssert.AreEqual(new[] { 10, 20, 30 }, result);
        }

        [TestMethod]
        public void GetColumn_T_EmptyColumn_ReturnsEmptyEnumerable()
        {
            var protocol = Substitute.For<SLProtocol>();
            uint colIdx = 0;
            protocol.NotifyProtocol(321, TableId, Arg.Is<object>(o => ((uint[])o).SequenceEqual(new[] { colIdx })))
                    .Returns(new object[] { new object[] { } });

            List<int> result = protocol.GetColumn<int>(TableId, colIdx).ToList();

            Assert.AreEqual(0, result.Count);
        }

        [TestMethod]
        public void GetColumn_T_NullValues_ReturnDefaults()
        {
            var protocol = Substitute.For<SLProtocol>();
            uint colIdx = 0;
            // DataMiner represents uninitialized cells as null; ChangeType<T> converts null to default(T).
            protocol.NotifyProtocol(321, TableId, Arg.Is<object>(o => ((uint[])o).SequenceEqual(new[] { colIdx })))
                    .Returns(new object[] { new object[] { null, null } });

            List<int> result = protocol.GetColumn<int>(TableId, colIdx).ToList();

            CollectionAssert.AreEqual(new[] { 0, 0 }, result);
        }

        [TestMethod]
        public void GetColumns_T2_InvokesNotifyProtocol321WithCorrectArguments()
        {
            var protocol = Substitute.For<SLProtocol>();
            var indices = new uint[] { 0, 1 };
            protocol.NotifyProtocol(321, TableId, indices)
                    .Returns(new object[] { new object[] { "key1", "key2" }, new object[] { "10", "20" } });

            List<string> result = protocol.GetColumns<string, int, string>(TableId, indices, (k, v) => $"{k}:{v}").ToList();

            protocol.Received(1).NotifyProtocol(321, TableId, indices);

            CollectionAssert.AreEqual(new[] { "key1:10", "key2:20" }, result);
        }

        [TestMethod]
        public void GetColumns_T2_NullIndices_ThrowsArgumentNullException()
        {
            var protocol = Substitute.For<SLProtocol>();
            Assert.ThrowsException<ArgumentNullException>(() => protocol.GetColumns<string, int, string>(TableId, null, (k, v) => String.Empty).ToList());
        }

        [TestMethod]
        public void GetColumns_T2_NullSelector_ThrowsArgumentNullException()
        {
            var protocol = Substitute.For<SLProtocol>();
            Assert.ThrowsException<ArgumentNullException>(() => protocol.GetColumns<string, int, string>(TableId, new uint[] { 0, 1 }, (Func<string, int, string>)null).ToList());
        }

        [TestMethod]
        public void GetColumns_T2_WrongColumnCount_ThrowsArgumentOutOfRangeException()
        {
            var protocol = Substitute.For<SLProtocol>();
            Assert.ThrowsException<ArgumentOutOfRangeException>(() => protocol.GetColumns<string, int, string>(TableId, new uint[] { 0 }, (k, v) => String.Empty).ToList());
        }

        [TestMethod]
        public void GetColumns_T3_ReturnsSelectorResultForEveryRow()
        {
            var protocol = Substitute.For<SLProtocol>();
            var indices = new uint[] { 0, 1, 2 };
            protocol.NotifyProtocol(321, TableId, Arg.Is<object>(o => ((uint[])o).SequenceEqual(indices)))
                    .Returns(new object[] { new object[] { "key1", "key2" }, new object[] { "10", "20" }, new object[] { "true", "false" } });

            List<string> result = protocol.GetColumns<string, int, bool, string>(TableId, indices, (k, v, b) => $"{k}:{v}:{b}").ToList();

            protocol.Received(1).NotifyProtocol(321, TableId, indices);

            CollectionAssert.AreEqual(new[] { "key1:10:True", "key2:20:False" }, result);
        }

        [TestMethod]
        public void GetColumns_T3_NullIndices_ThrowsArgumentNullException()
        {
            var protocol = Substitute.For<SLProtocol>();
            Assert.ThrowsException<ArgumentNullException>(() => protocol.GetColumns<string, int, bool, string>(TableId, null, (k, v, b) => String.Empty).ToList());
        }

        [TestMethod]
        public void GetColumns_T3_NullSelector_ThrowsArgumentNullException()
        {
            var protocol = Substitute.For<SLProtocol>();
            Assert.ThrowsException<ArgumentNullException>(() => protocol.GetColumns<string, int, bool, string>(TableId, new uint[] { 0, 1, 2 }, (Func<string, int, bool, string>)null).ToList());
        }

        [TestMethod]
        public void GetColumns_T3_WrongColumnCount_ThrowsArgumentOutOfRangeException()
        {
            var protocol = Substitute.For<SLProtocol>();
            Assert.ThrowsException<ArgumentOutOfRangeException>(() => protocol.GetColumns<string, int, bool, string>(TableId, new uint[] { 0, 1 }, (k, v, b) => String.Empty).ToList());
        }

        [TestMethod]
        public void GetColumns_T4_InvokesNotifyProtocol321WithCorrectArguments()
        {
            var protocol = Substitute.For<SLProtocol>();
            var indices = new uint[] { 0, 1, 2, 3 };
            protocol.NotifyProtocol(321, TableId, indices)
                    .Returns(new object[] { new object[] { "k" }, new object[] { "1" }, new object[] { "2" }, new object[] { "3" } });

            List<string> result = protocol.GetColumns<string, int, int, int, string>(TableId, indices, (a, b, c, d) => $"{a}:{b}:{c}:{d}").ToList();

            protocol.Received(1).NotifyProtocol(321, TableId, indices);

            CollectionAssert.AreEqual(new[] { "k:1:2:3" }, result);
        }

        [TestMethod]
        public void GetColumns_T4_NullIndices_ThrowsArgumentNullException()
        {
            var protocol = Substitute.For<SLProtocol>();
            Assert.ThrowsException<ArgumentNullException>(() => protocol.GetColumns<string, int, int, int, string>(TableId, null, (a, b, c, d) => String.Empty).ToList());
        }

        [TestMethod]
        public void GetColumns_T4_NullSelector_ThrowsArgumentNullException()
        {
            var protocol = Substitute.For<SLProtocol>();
            Assert.ThrowsException<ArgumentNullException>(() => protocol.GetColumns<string, int, int, int, string>(TableId, new uint[] { 0, 1, 2, 3 }, (Func<string, int, int, int, string>)null).ToList());
        }

        [TestMethod]
        public void GetColumns_T4_WrongColumnCount_ThrowsArgumentOutOfRangeException()
        {
            var protocol = Substitute.For<SLProtocol>();
            Assert.ThrowsException<ArgumentOutOfRangeException>(() => protocol.GetColumns<string, int, int, int, string>(TableId, new uint[] { 0, 1, 2 }, (a, b, c, d) => String.Empty).ToList());
        }

        [TestMethod]
        public void GetColumns_T5_InvokesNotifyProtocol321WithCorrectArguments()
        {
            var protocol = Substitute.For<SLProtocol>();
            var indices = new uint[] { 0, 1, 2, 3, 4 };
            protocol.NotifyProtocol(321, TableId, indices)
                    .Returns(new object[] { new object[] { "k" }, new object[] { "1" }, new object[] { "2" }, new object[] { "3" }, new object[] { "4" } });

            List<string> result = protocol.GetColumns<string, int, int, int, int, string>(TableId, indices, (a, b, c, d, e) => $"{a}:{b}:{c}:{d}:{e}").ToList();

            protocol.Received(1).NotifyProtocol(321, TableId, indices);

            CollectionAssert.AreEqual(new[] { "k:1:2:3:4" }, result);
        }

        [TestMethod]
        public void GetColumns_T5_NullIndices_ThrowsArgumentNullException()
        {
            var protocol = Substitute.For<SLProtocol>();
            Assert.ThrowsException<ArgumentNullException>(() => protocol.GetColumns<string, int, int, int, int, string>(TableId, null, (a, b, c, d, e) => String.Empty).ToList());
        }

        [TestMethod]
        public void GetColumns_T5_NullSelector_ThrowsArgumentNullException()
        {
            var protocol = Substitute.For<SLProtocol>();
            Assert.ThrowsException<ArgumentNullException>(() => protocol.GetColumns<string, int, int, int, int, string>(TableId, new uint[] { 0, 1, 2, 3, 4 }, (Func<string, int, int, int, int, string>)null).ToList());
        }

        [TestMethod]
        public void GetColumns_T5_WrongColumnCount_ThrowsArgumentOutOfRangeException()
        {
            var protocol = Substitute.For<SLProtocol>();
            Assert.ThrowsException<ArgumentOutOfRangeException>(() => protocol.GetColumns<string, int, int, int, int, string>(TableId, new uint[] { 0, 1, 2, 3 }, (a, b, c, d, e) => String.Empty).ToList());
        }

        [TestMethod]
        public void GetColumns_IEnumerable_InvokesNotifyProtocol321WithCorrectArguments()
        {
            var protocol = Substitute.For<SLProtocol>();
            var indices = new uint[] { 0, 1 };
            // GetColumns calls columnsIdx.ToArray() internally, so the actual NotifyProtocol call
            // receives a new array instance with the same content; match it by value with Arg.Is.
            protocol.NotifyProtocol(321, TableId, Arg.Any<object>())
                    .Returns(new object[] { new object[] { "a1", "a2" }, new object[] { "b1", "b2" } });

            object[] result = protocol.GetColumns(TableId, (IEnumerable<uint>)indices);

            protocol.Received(1).NotifyProtocol(
                321,
                TableId,
                Arg.Is<object>(o => ((uint[])o).SequenceEqual(indices)));

            Assert.AreEqual(2, result.Length);
            CollectionAssert.AreEqual(new object[] { "a1", "a2" }, (object[])result[0]);
            CollectionAssert.AreEqual(new object[] { "b1", "b2" }, (object[])result[1]);
        }

        [TestMethod]
        public void GetColumns_IEnumerable_EmptyIndices_ReturnsEmptyArray()
        {
            var protocol = Substitute.For<SLProtocol>();

            object[] result = protocol.GetColumns(TableId, Enumerable.Empty<uint>());

            Assert.AreEqual(0, result.Length);
            protocol.DidNotReceive().NotifyProtocol(Arg.Any<int>(), Arg.Any<object>(), Arg.Any<object>());
        }

        [TestMethod]
        public void GetColumns_IEnumerable_NullIndices_ThrowsArgumentNullException()
        {
            var protocol = Substitute.For<SLProtocol>();
            Assert.ThrowsException<ArgumentNullException>(() => protocol.GetColumns(TableId, (IEnumerable<uint>)null));
        }

        [TestMethod]
        public void GetParameter_T_ReturnsValueConvertedToRequestedType()
        {
            var protocol = Substitute.For<SLProtocol>();
            protocol.GetParameter(100).Returns("42");

            int result = protocol.GetParameter<int>(100);

            Assert.AreEqual(42, result);
        }

        [TestMethod]
        public void GetParameter_T_NullValue_ThrowsInvalidOperationException()
        {
            var protocol = Substitute.For<SLProtocol>();
            protocol.GetParameter(100).Returns((object)null);

            Assert.ThrowsException<InvalidOperationException>(() => protocol.GetParameter<int>(100));
        }

        [TestMethod]
        public void GetParameter_T_DelegatesToSLProtocolGetParameter()
        {
            var protocol = Substitute.For<SLProtocol>();
            protocol.GetParameter(200).Returns("7");

            protocol.GetParameter<int>(200);

            protocol.Received(1).GetParameter(200);
        }

        [TestMethod]
        public void GetParameters_T2_ValuesAreConvertedToRequestedTypes()
        {
            var protocol = Substitute.For<SLProtocol>();
            var ids = new uint[] { 1, 2 };
            protocol.GetParameters(ids).Returns(new object[] { "10", "hello" });

            protocol.GetParameters(ids, out int p1, out string p2);

            protocol.Received(1).GetParameters(ids);

            Assert.AreEqual(10, p1);
            Assert.AreEqual("hello", p2);
        }

        [TestMethod]
        public void GetParameters_T2_NullIds_ThrowsArgumentNullException()
        {
            var protocol = Substitute.For<SLProtocol>();
            Assert.ThrowsException<ArgumentNullException>(() => protocol.GetParameters<int, string>(null, out _, out _));
        }

        [TestMethod]
        public void GetParameters_T2_WrongIdCount_ThrowsArgumentOutOfRangeException()
        {
            var protocol = Substitute.For<SLProtocol>();
            Assert.ThrowsException<ArgumentOutOfRangeException>(() => protocol.GetParameters<int, string>(new uint[] { 1 }, out _, out _));
        }

        [TestMethod]
        public void GetParameters_T3_ValuesAreConvertedToRequestedTypes()
        {
            var protocol = Substitute.For<SLProtocol>();
            var ids = new uint[] { 1, 2, 3 };
            protocol.GetParameters(ids).Returns(new object[] { "1", "99", "true" });

            protocol.GetParameters(ids, out int p1, out double p2, out bool p3);

            protocol.Received(1).GetParameters(ids);

            Assert.AreEqual(1, p1);
            Assert.AreEqual(99.0, p2);
            Assert.IsTrue(p3);
        }

        [TestMethod]
        public void GetParameters_T3_NullIds_ThrowsArgumentNullException()
        {
            var protocol = Substitute.For<SLProtocol>();
            Assert.ThrowsException<ArgumentNullException>(() => protocol.GetParameters<int, int, int>(null, out _, out _, out _));
        }

        [TestMethod]
        public void GetParameters_T3_WrongIdCount_ThrowsArgumentOutOfRangeException()
        {
            var protocol = Substitute.For<SLProtocol>();
            Assert.ThrowsException<ArgumentOutOfRangeException>(() => protocol.GetParameters<int, int, int>(new uint[] { 1, 2 }, out _, out _, out _));
        }

        [TestMethod]
        public void GetParameters_T4_ValuesAreConvertedToRequestedTypes()
        {
            var protocol = Substitute.For<SLProtocol>();
            var ids = new uint[] { 1, 2, 3, 4 };
            protocol.GetParameters(ids).Returns(new object[] { "1", "2", "3", "4" });

            protocol.GetParameters(ids, out int p1, out int p2, out int p3, out int p4);

            protocol.Received(1).GetParameters(ids);

            Assert.AreEqual(1, p1);
            Assert.AreEqual(2, p2);
            Assert.AreEqual(3, p3);
            Assert.AreEqual(4, p4);
        }

        [TestMethod]
        public void GetParameters_T4_NullIds_ThrowsArgumentNullException()
        {
            var protocol = Substitute.For<SLProtocol>();
            Assert.ThrowsException<ArgumentNullException>(() => protocol.GetParameters<int, int, int, int>(null, out _, out _, out _, out _));
        }

        [TestMethod]
        public void GetParameters_T4_WrongIdCount_ThrowsArgumentOutOfRangeException()
        {
            var protocol = Substitute.For<SLProtocol>();
            Assert.ThrowsException<ArgumentOutOfRangeException>(() => protocol.GetParameters<int, int, int, int>(new uint[] { 1, 2, 3 }, out _, out _, out _, out _));
        }

        [TestMethod]
        public void GetParameters_T5_ValuesAreConvertedToRequestedTypes()
        {
            var protocol = Substitute.For<SLProtocol>();
            var ids = new uint[] { 1, 2, 3, 4, 5 };
            protocol.GetParameters(ids).Returns(new object[] { "1", "2", "3", "4", "5" });

            protocol.GetParameters(ids, out int p1, out int p2, out int p3, out int p4, out int p5);

            protocol.Received(1).GetParameters(ids);

            Assert.AreEqual(1, p1);
            Assert.AreEqual(2, p2);
            Assert.AreEqual(3, p3);
            Assert.AreEqual(4, p4);
            Assert.AreEqual(5, p5);
        }

        [TestMethod]
        public void GetParameters_T5_NullIds_ThrowsArgumentNullException()
        {
            var protocol = Substitute.For<SLProtocol>();
            Assert.ThrowsException<ArgumentNullException>(() => protocol.GetParameters<int, int, int, int, int>(null, out _, out _, out _, out _, out _));
        }

        [TestMethod]
        public void GetParameters_T5_WrongIdCount_ThrowsArgumentOutOfRangeException()
        {
            var protocol = Substitute.For<SLProtocol>();
            Assert.ThrowsException<ArgumentOutOfRangeException>(() => protocol.GetParameters<int, int, int, int, int>(new uint[] { 1, 2, 3, 4 }, out _, out _, out _, out _, out _));
        }

        [TestMethod]
        public void GetParameters_T6_ValuesAreConvertedToRequestedTypes()
        {
            var protocol = Substitute.For<SLProtocol>();
            var ids = new uint[] { 1, 2, 3, 4, 5, 6 };
            protocol.GetParameters(ids).Returns(new object[] { "1", "2", "3", "4", "5", "6" });

            protocol.GetParameters(ids, out int p1, out int p2, out int p3, out int p4, out int p5, out int p6);

            protocol.Received(1).GetParameters(ids);

            Assert.AreEqual(1, p1);
            Assert.AreEqual(2, p2);
            Assert.AreEqual(3, p3);
            Assert.AreEqual(4, p4);
            Assert.AreEqual(5, p5);
            Assert.AreEqual(6, p6);
        }

        [TestMethod]
        public void GetParameters_T6_NullIds_ThrowsArgumentNullException()
        {
            var protocol = Substitute.For<SLProtocol>();
            Assert.ThrowsException<ArgumentNullException>(() => protocol.GetParameters<int, int, int, int, int, int>(null, out _, out _, out _, out _, out _, out _));
        }

        [TestMethod]
        public void GetParameters_T6_WrongIdCount_ThrowsArgumentOutOfRangeException()
        {
            var protocol = Substitute.For<SLProtocol>();
            Assert.ThrowsException<ArgumentOutOfRangeException>(() => protocol.GetParameters<int, int, int, int, int, int>(new uint[] { 1, 2, 3, 4, 5 }, out _, out _, out _, out _, out _, out _));
        }

        [TestMethod]
        public void GetParameters_T7_ValuesAreConvertedToRequestedTypes()
        {
            var protocol = Substitute.For<SLProtocol>();
            var ids = new uint[] { 1, 2, 3, 4, 5, 6, 7 };
            protocol.GetParameters(ids).Returns(new object[] { "1", "2", "3", "4", "5", "6", "7" });

            protocol.GetParameters(ids, out int p1, out int p2, out int p3, out int p4, out int p5, out int p6, out int p7);

            protocol.Received(1).GetParameters(ids);

            Assert.AreEqual(1, p1);
            Assert.AreEqual(2, p2);
            Assert.AreEqual(3, p3);
            Assert.AreEqual(4, p4);
            Assert.AreEqual(5, p5);
            Assert.AreEqual(6, p6);
            Assert.AreEqual(7, p7);
        }

        [TestMethod]
        public void GetParameters_T7_NullIds_ThrowsArgumentNullException()
        {
            var protocol = Substitute.For<SLProtocol>();
            Assert.ThrowsException<ArgumentNullException>(() => protocol.GetParameters<int, int, int, int, int, int, int>(null, out _, out _, out _, out _, out _, out _, out _));
        }

        [TestMethod]
        public void GetParameters_T7_WrongIdCount_ThrowsArgumentOutOfRangeException()
        {
            var protocol = Substitute.For<SLProtocol>();
            Assert.ThrowsException<ArgumentOutOfRangeException>(() => protocol.GetParameters<int, int, int, int, int, int, int>(new uint[] { 1, 2, 3, 4, 5, 6 }, out _, out _, out _, out _, out _, out _, out _));
        }

        [TestMethod]
        public void GetParameters_T8_ValuesAreConvertedToRequestedTypes()
        {
            var protocol = Substitute.For<SLProtocol>();
            var ids = new uint[] { 1, 2, 3, 4, 5, 6, 7, 8 };
            protocol.GetParameters(ids).Returns(new object[] { "1", "2", "3", "4", "5", "6", "7", "8" });

            protocol.GetParameters(ids, out int p1, out int p2, out int p3, out int p4, out int p5, out int p6, out int p7, out int p8);

            protocol.Received(1).GetParameters(ids);

            Assert.AreEqual(1, p1);
            Assert.AreEqual(2, p2);
            Assert.AreEqual(3, p3);
            Assert.AreEqual(4, p4);
            Assert.AreEqual(5, p5);
            Assert.AreEqual(6, p6);
            Assert.AreEqual(7, p7);
            Assert.AreEqual(8, p8);
        }

        [TestMethod]
        public void GetParameters_T8_NullIds_ThrowsArgumentNullException()
        {
            var protocol = Substitute.For<SLProtocol>();
            Assert.ThrowsException<ArgumentNullException>(() => protocol.GetParameters<int, int, int, int, int, int, int, int>(null, out _, out _, out _, out _, out _, out _, out _, out _));
        }

        [TestMethod]
        public void GetParameters_T8_WrongIdCount_ThrowsArgumentOutOfRangeException()
        {
            var protocol = Substitute.For<SLProtocol>();
            Assert.ThrowsException<ArgumentOutOfRangeException>(() => protocol.GetParameters<int, int, int, int, int, int, int, int>(new uint[] { 1, 2, 3, 4, 5, 6, 7 }, out _, out _, out _, out _, out _, out _, out _, out _));
        }

        [TestMethod]
        public void GetParameters_T9_ValuesAreConvertedToRequestedTypes()
        {
            var protocol = Substitute.For<SLProtocol>();
            var ids = new uint[] { 1, 2, 3, 4, 5, 6, 7, 8, 9 };
            protocol.GetParameters(ids).Returns(new object[] { "1", "2", "3", "4", "5", "6", "7", "8", "9" });

            protocol.GetParameters(ids, out int p1, out int p2, out int p3, out int p4, out int p5, out int p6, out int p7, out int p8, out int p9);

            protocol.Received(1).GetParameters(ids);

            Assert.AreEqual(1, p1);
            Assert.AreEqual(2, p2);
            Assert.AreEqual(3, p3);
            Assert.AreEqual(4, p4);
            Assert.AreEqual(5, p5);
            Assert.AreEqual(6, p6);
            Assert.AreEqual(7, p7);
            Assert.AreEqual(8, p8);
            Assert.AreEqual(9, p9);
        }

        [TestMethod]
        public void GetParameters_T9_NullIds_ThrowsArgumentNullException()
        {
            var protocol = Substitute.For<SLProtocol>();
            Assert.ThrowsException<ArgumentNullException>(() => protocol.GetParameters<int, int, int, int, int, int, int, int, int>(null, out _, out _, out _, out _, out _, out _, out _, out _, out _));
        }

        [TestMethod]
        public void GetParameters_T9_WrongIdCount_ThrowsArgumentOutOfRangeException()
        {
            var protocol = Substitute.For<SLProtocol>();
            Assert.ThrowsException<ArgumentOutOfRangeException>(() => protocol.GetParameters<int, int, int, int, int, int, int, int, int>(new uint[] { 1, 2, 3, 4, 5, 6, 7, 8 }, out _, out _, out _, out _, out _, out _, out _, out _, out _));
        }

        [TestMethod]
        public void GetParameters_T10_ValuesAreConvertedToRequestedTypes()
        {
            var protocol = Substitute.For<SLProtocol>();
            var ids = new uint[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 };
            protocol.GetParameters(ids).Returns(new object[] { "1", "2", "3", "4", "5", "6", "7", "8", "9", "10" });

            protocol.GetParameters(ids, out int p1, out int p2, out int p3, out int p4, out int p5, out int p6, out int p7, out int p8, out int p9, out int p10);

            protocol.Received(1).GetParameters(ids);

            Assert.AreEqual(1, p1);
            Assert.AreEqual(2, p2);
            Assert.AreEqual(3, p3);
            Assert.AreEqual(4, p4);
            Assert.AreEqual(5, p5);
            Assert.AreEqual(6, p6);
            Assert.AreEqual(7, p7);
            Assert.AreEqual(8, p8);
            Assert.AreEqual(9, p9);
            Assert.AreEqual(10, p10);
        }

        [TestMethod]
        public void GetParameters_T10_NullIds_ThrowsArgumentNullException()
        {
            var protocol = Substitute.For<SLProtocol>();
            Assert.ThrowsException<ArgumentNullException>(() => protocol.GetParameters<int, int, int, int, int, int, int, int, int, int>(null, out _, out _, out _, out _, out _, out _, out _, out _, out _, out _));
        }

        [TestMethod]
        public void GetParameters_T10_WrongIdCount_ThrowsArgumentOutOfRangeException()
        {
            var protocol = Substitute.For<SLProtocol>();
            Assert.ThrowsException<ArgumentOutOfRangeException>(() => protocol.GetParameters<int, int, int, int, int, int, int, int, int, int>(new uint[] { 1, 2, 3, 4, 5, 6, 7, 8, 9 }, out _, out _, out _, out _, out _, out _, out _, out _, out _, out _));
        }
    }
}