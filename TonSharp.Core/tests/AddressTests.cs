namespace TonSharp.Core.Tests
{
    internal class AddressTests
    {
        [Test]
        public void Parse_RawBaseChain()
        {
            string expected = "0:57d423f9adfa10a6715b3a2c29e6f84f1736cf34e829258937a5238390920cc6";
            Address address = Address.ParseRaw(expected);
            Assert.That(address.ToRawString(), Is.EqualTo(expected));
        }

        [Test]
        public void Parse_FriendlyBaseChain()
        {
            string expected = "EQBX1CP5rfoQpnFbOiwp5vhPFzbPNOgpJYk3pSODkJIMxiU3";
            Address address = Address.ParseFriendly(expected);
            Assert.That(address.ToString(), Is.EqualTo(expected));
        }

        [Test]
        public void Create_FromContract()
        {
            string expected = "EQBX1CP5rfoQpnFbOiwp5vhPFzbPNOgpJYk3pSODkJIMxiU3";
            Slice slice = Cell.FromBase64("te6cckECJwEABBQAAgE0EwEDE4AAAAAAAAAAABAQAwIACwAAA-iAEAEU_wD0pBP0vPLICwQCAWIGBQAJoR-f4AUCAs4KBwIBIAkIAB0A8jLP1jPFgHPFszJ7VSAAOztRNDTP_pAINdJwgCafwH6QNQwECQQI-AwcFltbYAIBIAwLABE-kQwcLry4U2AC1wyIccAkl8D4NDTAwFxsJJfA-D6QPpAMfoAMXHXIfoAMfoAMPACBLOOFDBsIjRSMscF8uGVAfpA1DAQI_AD4AbTH9M_ghBfzD0UUjC6jocyEDdeMkAT4DA0NDU1ghAvyyaiErrjAl8EhA_y8IA4NAHJwghCLdxc1BcjL_1AEzxYQJIBAcIAQyMsFUAfPFlAF-gIVy2oSyx_LPyJus5RYzxcBkTLiAckB-wAB9lE1xwXy4ZH6QCHwAfpA0gAx-gCCCvrwgBuhIZRTFaCh3iLXCwHDACCSBqGRNuIgwv_y4ZIhjj6CEAUTjZHIUAnPFlALzxZxJEkUVEagcIAQyMsFUAfPFlAF-gIVy2oSyx_LPyJus5RYzxcBkTLiAckB-wAQR5QQKjdb4g8AggKONSbwAYIQ1TJ22xA3RABtcXCAEMjLBVAHzxZQBfoCFctqEssfyz8ibrOUWM8XAZEy4gHJAfsAkzAyNOJVAvADAgASEQAAAAIBART_APSkE_S88sgLFAIBYhwVAgEgFxYAJbyC32omh9IGmf6mpqGC3oahgsQCASAbGAIBIBoZAC209H2omh9IGmf6mpqGAovgngCOAD4AsAAvtdr9qJofSBpn-pqahg2IOhph-mH_SAYQAEO4tdMe1E0PpA0z_U1NQwECRfBNDUMdQw0HHIywcBzxbMyYAgLNIh0CASAfHgA9Ra8ARwIfAFd4AYyMsFWM8WUAT6AhPLaxLMzMlx-wCAIBICEgABs-QB0yMsCEsoHy__J0IAAtAHIyz_4KM8WyXAgyMsBE_QA9ADLAMmAE59EGOASK3wAOhpgYC42Eit8H0gGADpj-mf9qJofSBpn-pqahhBCDSenKgpQF1HFBuvgoDoQQhUZYBWuEAIZGWCqALnixJ9AQpltQnlj-WfgOeLZMAgfYBwGyi544L5cMiS4ADxgRLgAXGBEuAB8YEYGYHgAkJiUkIwA8jhXU1DAQNEEwyFAFzxYTyz_MzMzJ7VTgXwSED_LwACwyNAH6QDBBRMhQBc8WE8s_zMzMye1UAKY1cAPUMI43gED0lm-lII4pBqQggQD6vpPywY_egQGTIaBTJbvy9AL6ANQwIlRLMPAGI7qTAqQC3gSSbCHis-YwMlBEQxPIUAXPFhPLP8zMzMntVABgNQLTP1MTu_LhklMTugH6ANQwKBA0WfAGjhIBpENDyFAFzxYTyz_MzMzJ7VSSXwXiv2QYLQ==").BeginParse();
            StateInit init = slice.Load<StateInit>();
            Address address = Address.FromContract(0, init);
            Assert.That(address.ToString(), Is.EqualTo(expected));
        }
    }
}
