namespace TonSharp.Core.Tests
{
    internal class CellSerializationsTests
    {
        [Test]
        public void Empty()
        {
            string targetBoc = "b5ee9c72010101010002000000";
            Cell cell = Cell.Begin().End();
            string hex = cell.ToBoc().ToHexString();
            Assert.That(hex, Is.EqualTo(targetBoc));

            Slice slice = Cell.FromHex(targetBoc).BeginParse();
            Assert.Multiple(() =>
            {
                Assert.That(slice.RemainingBits, Is.EqualTo(0));
                Assert.That(slice.RemainingRefs, Is.EqualTo(0));
            });
        }

        [Test]
        public void WithBits()
        {
            string targetBoc = "b5ee9c720101010100030000027c";
            Cell cell = Cell.Begin()
                .StoreUInt(1, 2)
                .StoreInt(-4, 6)
                .End();
            string hex = cell.ToBoc().ToHexString();
            Assert.That(hex, Is.EqualTo(targetBoc));

            Slice slice = Cell.FromHex(targetBoc).BeginParse();
            Assert.Multiple(() =>
            {
                Assert.That(slice.RemainingBits, Is.EqualTo(8));
                Assert.That(slice.RemainingRefs, Is.EqualTo(0));
                Assert.That(slice.LoadUInt<int>(2), Is.EqualTo(1));
                Assert.That(slice.LoadInt<int>(6), Is.EqualTo(-4));
            });
        }

        [Test]
        public void WithBits_MoreThanAvailableMaxValue()
        {
            string targetBoc = "b5ee9c7201010101000f000019000000000008bffffffffffd68";
            Cell cell = Cell.Begin()
                .StoreUInt(69, 51)
                .StoreInt(-42, 49)
                .End();
            string hex = cell.ToBoc().ToHexString();
            Assert.That(hex, Is.EqualTo(targetBoc));

            Slice slice = Cell.FromHex(targetBoc).BeginParse();
            Assert.Multiple(() =>
            {
                Assert.That(slice.RemainingBits, Is.EqualTo(51 + 49));
                Assert.That(slice.RemainingRefs, Is.EqualTo(0));
                Assert.That(slice.LoadUInt<ulong>(51), Is.EqualTo(69));
                Assert.That(slice.LoadInt<long>(49), Is.EqualTo(-42));
            });
        }

        [Test]
        public void WithBits_AtMax()
        {
            string targetBoc = "b5ee9c720101010100060000077ff00010";
            Cell cell = Cell.Begin()
                .StoreUInt<byte>(255, 9)
                .StoreInt(short.MinValue, 18)
                .End();
            string hex = cell.ToBoc().ToHexString();
            Assert.That(hex, Is.EqualTo(targetBoc));

            Slice slice = Cell.FromHex(targetBoc).BeginParse();
            Assert.Multiple(() =>
            {
                Assert.That(slice.RemainingBits, Is.EqualTo(9 + 18));
                Assert.That(slice.RemainingRefs, Is.EqualTo(0));
                Assert.That(slice.LoadUInt<ushort>(9), Is.EqualTo(255));
                Assert.That(slice.LoadInt<int>(18), Is.EqualTo(short.MinValue));
            });
        }

        [Test]
        public void WithRefs()
        {
            string targetBoc = "b5ee9c7201010201000600020001010000";
            Cell cell = Cell.Begin()
                .StoreRef(Cell.Begin().End())
                .StoreRef(Cell.Begin().End())
                .End();
            string hex = cell.ToBoc().ToHexString();
            Assert.That(hex, Is.EqualTo(targetBoc));

            Slice slice = Cell.FromHex(targetBoc).BeginParse();
            Assert.Multiple(() =>
            {
                Assert.That(slice.RemainingBits, Is.EqualTo(0));
                Assert.That(slice.RemainingRefs, Is.EqualTo(2));
                Cell cell0 = slice.LoadRef();
                Assert.That(cell0.Bits.Length, Is.EqualTo(0));
                Cell cell1 = slice.LoadRef();
                Assert.That(cell1.Bits.Length, Is.EqualTo(0));
            });
        }

        [Test]
        public void WithRefs_WithBits()
        {
            string targetBoc = "b5ee9c7201010301000e000203016001020001ea000501b398";
            Cell cell = Cell.Begin()
                .StoreUInt(5, 10)
                .StoreRef(Cell.Begin().StoreBit(true).StoreInt(-6, 5).End())
                .StoreRef(Cell.Begin().StoreUInt(6969, 20).End())
                .End();
            string hex = cell.ToBoc().ToHexString();
            Assert.That(hex, Is.EqualTo(targetBoc));

            Slice slice = Cell.FromHex(targetBoc).BeginParse();
            Assert.Multiple(() =>
            {
                Assert.That(slice.RemainingBits, Is.EqualTo(10));
                Assert.That(slice.RemainingRefs, Is.EqualTo(2));
                Slice cell0 = slice.LoadRef().BeginParse();
                Assert.That(cell0.RemainingBits, Is.EqualTo(1 + 5));
                Assert.That(cell0.LoadBit(), Is.EqualTo(true));
                Assert.That(cell0.LoadInt<int>(5), Is.EqualTo(-6));
                Slice cell1 = slice.LoadRef().BeginParse();
                Assert.That(cell1.RemainingBits, Is.EqualTo(20));
                Assert.That(cell1.LoadUInt<uint>(20), Is.EqualTo(6969));
            });
        }

        [Test]
        public void StateInit()
        {
            Cell cell = Cell.FromBase64("te6ccsECFgEAAwQAAAAABQAwAD0AQgDEAMsBAwE9AXYBewGAAa8BtAG/AcQByQHYAecCCAJ/AsYCATQCAQBRAAAAACmpoxc5EfKaT1Mk8yrEGec/Fn2lnJOdZbXHFNgNVkkMOWI7J0ABFP8A9KQT9LzyyAsDAgEgCQQE+PKDCNcYINMf0x/THwL4I7vyZO1E0NMf0x/T//QE0VFDuvKhUVG68qIF+QFUEGT5EPKj+AAkpMjLH1JAyx9SMMv/UhD0AMntVPgPAdMHIcAAn2xRkyDXSpbTB9QC+wDoMOAhwAHjACHAAuMAAcADkTDjDQOkyMsfEssfy/8IBwYFAAr0AMntVABsgQEI1xj6ANM/MFIkgQEI9Fnyp4IQZHN0cnB0gBjIywXLAlAFzxZQA/oCE8tqyx8Syz/Jc/sAAHCBAQjXGPoA0z/IVCBHgQEI9FHyp4IQbm90ZXB0gBjIywXLAlAGzxZQBPoCFMtqEssfyz/Jc/sAAgBu0gf6ANTUIvkABcjKBxXL/8nQd3SAGMjLBcsCIs8WUAX6AhTLaxLMzMlz+wDIQBSBAQj0UfKnAgIBSBMKAgEgDAsAWb0kK29qJoQICga5D6AhhHDUCAhHpJN9KZEM5pA+n/mDeBKAG3gQFImHFZ8xhAIBIA4NABG4yX7UTQ1wsfgCAVgSDwIBIBEQABmvHfaiaEAQa5DrhY/AABmtznaiaEAga5Drhf/AAD2ynftRNCBAUDXIfQEMALIygfL/8nQAYEBCPQKb6ExgAubQAdDTAyFxsJJfBOAi10nBIJJfBOAC0x8hghBwbHVnvSKCEGRzdHK9sJJfBeAD+kAwIPpEAcjKB8v/ydDtRNCBAUDXIfQEMFyBAQj0Cm+hMbOSXwfgBdM/yCWCEHBsdWe6kjgw4w0DghBkc3RyupJfBuMNFRQAilAEgQEI9Fkw7UTQgQFA1yDIAc8W9ADJ7VQBcrCOI4IQZHN0coMesXCAGFAFywVQA88WI/oCE8tqyx/LP8mAQPsAkl8D4gB4AfoA9AQw+CdvIjBQCqEhvvLgUIIQcGx1Z4MesXCAGFAEywUmzxZY+gIZ9ADLaRfLH1Jgyz8gyYBA+wAGxQEyaQ==");
        }
    }
}