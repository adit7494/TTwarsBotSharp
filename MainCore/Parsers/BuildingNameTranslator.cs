namespace MainCore.Parsers
{
    /// <summary>
    /// Translates building names from various languages to English BuildingEnums.
    /// TTWars servers may use different languages (Indonesian, etc.)
    /// </summary>
    public static class BuildingNameTranslator
    {
        private static readonly Dictionary<string, BuildingEnums> _translations = new(StringComparer.OrdinalIgnoreCase)
        {
            // Indonesian translations - Complete list based on Travian Indonesian server
            // Resource Fields (gid1-4)
            ["PenebangKayu"] = BuildingEnums.Woodcutter,
            ["TambangKayu"] = BuildingEnums.Woodcutter,
            ["PenggalianTanahLiat"] = BuildingEnums.ClayPit,
            ["TambangTanahLiat"] = BuildingEnums.ClayPit,
            ["TambangBesi"] = BuildingEnums.IronMine,
            ["Ladang"] = BuildingEnums.Cropland,
            ["Sawah"] = BuildingEnums.Cropland,

            // Resource Bonus Buildings (gid5-8)
            ["GergajiKayu"] = BuildingEnums.Sawmill,
            ["PabrikKayu"] = BuildingEnums.Sawmill,
            ["PabrikBata"] = BuildingEnums.Brickyard,
            ["PembakarBatu"] = BuildingEnums.Brickyard,
            ["PeleburanBesi"] = BuildingEnums.IronFoundry,
            ["PendulangBesi"] = BuildingEnums.IronFoundry,
            ["PabrikTepung"] = BuildingEnums.GrainMill,
            ["KincirAngin"] = BuildingEnums.GrainMill,
            ["Roti"] = BuildingEnums.Bakery,
            ["TokoRoti"] = BuildingEnums.Bakery,

            // Storage Buildings (gid9-10)
            ["Gudang"] = BuildingEnums.Warehouse,
            ["Lumbung"] = BuildingEnums.Granary,

            // Military Buildings (gid11-14, 17-19, 21-22, 25-27, 29-30, 35-37, 42)
            ["PandaiBesi"] = BuildingEnums.Smithy,
            ["Tempa"] = BuildingEnums.Smithy,
            ["BengkelBesi"] = BuildingEnums.Smithy,
            ["Barak"] = BuildingEnums.Barracks,
            ["BarakTentara"] = BuildingEnums.Barracks,
            ["BarakBesar"] = BuildingEnums.GreatBarracks,
            ["KandangKuda"] = BuildingEnums.Stable,
            ["Kuda"] = BuildingEnums.Stable,
            ["KandangKudaBesar"] = BuildingEnums.GreatStable,
            ["Bengkel"] = BuildingEnums.Workshop,
            ["Perangkap"] = BuildingEnums.Trapper,
            ["Penjebak"] = BuildingEnums.Trapper,
            ["RumahSakit"] = BuildingEnums.Hospital,

            // Infrastructure Buildings (gid15-16, 20, 23-24, 28, 31-34, 38-41)
            ["BangunanUtama"] = BuildingEnums.MainBuilding,
            ["GedungUtama"] = BuildingEnums.MainBuilding,
            ["GedungPusat"] = BuildingEnums.MainBuilding,
            ["TitikTemu"] = BuildingEnums.RallyPoint,
            ["TitikKumpul"] = BuildingEnums.RallyPoint,
            ["TempatBerkumpul"] = BuildingEnums.RallyPoint,
            ["TempatKumpul"] = BuildingEnums.RallyPoint,
            ["Akademi"] = BuildingEnums.Academy,
            ["Sekolah"] = BuildingEnums.Academy,
            ["AlunAlunTurnamen"] = BuildingEnums.TournamentSquare,
            ["LapanganTurnamen"] = BuildingEnums.TournamentSquare,
            ["Pasar"] = BuildingEnums.Marketplace,
            ["Kedutaan"] = BuildingEnums.Embassy,
            ["KantorKedutaan"] = BuildingEnums.Embassy,
            ["BalaiKota"] = BuildingEnums.TownHall,
            ["TempatTinggal"] = BuildingEnums.Residence,
            ["RumahTinggal"] = BuildingEnums.Residence,
            ["Istana"] = BuildingEnums.Palace,
            ["IstanaRaja"] = BuildingEnums.Palace,
            ["Bendahara"] = BuildingEnums.Treasury,
            ["Perbendaharaan"] = BuildingEnums.Treasury,
            ["BendaharaKerajaan"] = BuildingEnums.Treasury,
            ["KantorPerdagangan"] = BuildingEnums.TradeOffice,
            ["KantorDagang"] = BuildingEnums.TradeOffice,
            ["Persembunyian"] = BuildingEnums.Cranny,
            ["LubangPersembunyian"] = BuildingEnums.Cranny,
            ["TempatPersembunyian"] = BuildingEnums.Cranny,

            // Wall Buildings (gid31-33, 38, 41)
            ["TembokKota"] = BuildingEnums.CityWall,
            ["TembokTanah"] = BuildingEnums.EarthWall,
            ["TembokKayu"] = BuildingEnums.Palisade,
            ["PagarKayu"] = BuildingEnums.Palisade,
            ["TembokBatu"] = BuildingEnums.StoneWall,
            ["TembokDadakan"] = BuildingEnums.MakeshiftWall,
            ["TembokPelindung"] = BuildingEnums.CityWall,

            // Special Buildings (gid20, 23-24, 28, 34, 38-40)
            ["BatuTukang"] = BuildingEnums.StonemasonsLodge,
            ["TukangBatu"] = BuildingEnums.StonemasonsLodge,
            ["PabrikBir"] = BuildingEnums.Brewery,
            ["PabrikMinuman"] = BuildingEnums.Brewery,
            ["MansionPahlawan"] = BuildingEnums.HerosMansion,
            ["GudangBesar"] = BuildingEnums.GreatWarehouse,
            ["LumbungBesar"] = BuildingEnums.GreatGranary,
            ["KudaMinum"] = BuildingEnums.HorseDrinkingTrough,
            ["TempatMinumKuda"] = BuildingEnums.HorseDrinkingTrough,
            ["PusatKomando"] = BuildingEnums.CommandCenter,
            ["SistemPengairan"] = BuildingEnums.Waterworks,
            ["SistemIrigasi"] = BuildingEnums.Waterworks,

            // Alternative spellings with spaces (will be concatenated)
            ["Penebang Kayu"] = BuildingEnums.Woodcutter,
            ["Tambang Kayu"] = BuildingEnums.Woodcutter,
            ["Penggalian Tanah Liat"] = BuildingEnums.ClayPit,
            ["Tambang Tanah Liat"] = BuildingEnums.ClayPit,
            ["Tambang Besi"] = BuildingEnums.IronMine,
            ["Gergaji Kayu"] = BuildingEnums.Sawmill,
            ["Pabrik Bata"] = BuildingEnums.Brickyard,
            ["Peleburan Besi"] = BuildingEnums.IronFoundry,
            ["Pabrik Tepung"] = BuildingEnums.GrainMill,
            ["Kincir Angin"] = BuildingEnums.GrainMill,
            ["Toko Roti"] = BuildingEnums.Bakery,
            ["Pandai Besi"] = BuildingEnums.Smithy,
            ["Alun-alun Turnamen"] = BuildingEnums.TournamentSquare,
            ["Lapangan Turnamen"] = BuildingEnums.TournamentSquare,
            ["Bangunan Utama"] = BuildingEnums.MainBuilding,
            ["Gedung Utama"] = BuildingEnums.MainBuilding,
            ["Titik Temu"] = BuildingEnums.RallyPoint,
            ["Titik Kumpul"] = BuildingEnums.RallyPoint,
            ["Tempat Berkumpul"] = BuildingEnums.RallyPoint,
            ["Kantor Perdagangan"] = BuildingEnums.TradeOffice,
            ["Kantor Kedutaan"] = BuildingEnums.Embassy,
            ["Barak Tentara"] = BuildingEnums.Barracks,
            ["Barak Besar"] = BuildingEnums.GreatBarracks,
            ["Kandang Kuda"] = BuildingEnums.Stable,
            ["Kandang Kuda Besar"] = BuildingEnums.GreatStable,
            ["Tembok Kota"] = BuildingEnums.CityWall,
            ["Tembok Tanah"] = BuildingEnums.EarthWall,
            ["Tembok Kayu"] = BuildingEnums.Palisade,
            ["Pagar Kayu"] = BuildingEnums.Palisade,
            ["Tembok Batu"] = BuildingEnums.StoneWall,
            ["Tembok Dadakan"] = BuildingEnums.MakeshiftWall,
            ["Tukang Batu"] = BuildingEnums.StonemasonsLodge,
            ["Batu Tukang"] = BuildingEnums.StonemasonsLodge,
            ["Pabrik Bir"] = BuildingEnums.Brewery,
            ["Pabrik Minuman"] = BuildingEnums.Brewery,
            ["Mansion Pahlawan"] = BuildingEnums.HerosMansion,
            ["Gudang Besar"] = BuildingEnums.GreatWarehouse,
            ["Lumbung Besar"] = BuildingEnums.GreatGranary,
            ["Kuda Minum"] = BuildingEnums.HorseDrinkingTrough,
            ["Tempat Minum Kuda"] = BuildingEnums.HorseDrinkingTrough,
            ["Pusat Komando"] = BuildingEnums.CommandCenter,
            ["Sistem Pengairan"] = BuildingEnums.Waterworks,
            ["Sistem Irigasi"] = BuildingEnums.Waterworks,
            ["Rumah Sakit"] = BuildingEnums.Hospital,

            // Spanish translations (for future support)
            ["MinaDeHierro"] = BuildingEnums.IronMine,
            ["Aserradero"] = BuildingEnums.Woodcutter,
            ["PozoDeArcilla"] = BuildingEnums.ClayPit,
            ["CampoDeTrigo"] = BuildingEnums.Cropland,
        };

        /// <summary>
        /// Translates a building name to BuildingEnums.
        /// Returns the translated enum if found, otherwise tries direct Enum.TryParse.
        /// </summary>
        public static bool TryTranslate(string buildingName, out BuildingEnums result)
        {
            // First try direct parse (English)
            if (Enum.TryParse(buildingName, false, out result))
                return true;

            // Try translation dictionary
            if (_translations.TryGetValue(buildingName, out result))
                return true;

            // Try case-insensitive match on enum names
            foreach (var enumValue in Enum.GetValues<BuildingEnums>())
            {
                if (string.Equals(enumValue.ToString(), buildingName, StringComparison.OrdinalIgnoreCase))
                {
                    result = enumValue;
                    return true;
                }
            }

            result = BuildingEnums.Unknown;
            return false;
        }
    }
}
