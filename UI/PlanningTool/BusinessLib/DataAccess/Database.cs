namespace BusinessLib
{
    /// <summary>
    /// A data source that provides raw data objects. 
    /// </summary>
    public static class Database
    {
         #region GetEngineConfiguration

        public static EngineObject GetFamilyTree()
        {
            // In a real app this method would access a database.
            return new EngineObject
            {
                Name = "David Weatherbeam",
                Children =
                {
                    new EngineObject
                    {
                        Name="Alberto Weatherbeam",
                        Children=
                        {
                            new EngineObject
                            {
                                Name="Zena Hairmonger",
                                Children=
                                {
                                    new EngineObject
                                    {
                                        Name="Sarah Applifunk",
                                    }
                                }
                            },
                            new EngineObject
                            {
                                Name="Jenny van Machoqueen",
                                Children=
                                {
                                    new EngineObject
                                    {
                                        Name="Nick van Machoqueen",
                                    },
                                    new EngineObject
                                    {
                                        Name="Matilda Porcupinicus",
                                    },
                                    new EngineObject
                                    {
                                        Name="Bronco van Machoqueen",
                                    }
                                }
                            }
                        }
                    },
                    new EngineObject
                    {
                        Name="Komrade Winkleford",
                        Children=
                        {
                            new EngineObject
                            {
                                Name="Maurice Winkleford",
                                Children=
                                {
                                    new EngineObject
                                    {
                                        Name="Divinity W. Llamafoot",
                                    }
                                }
                            },
                            new EngineObject
                            {
                                Name="Komrade Winkleford, Jr.",
                                Children=
                                {
                                    new EngineObject
                                    {
                                        Name="Saratoga Z. Crankentoe",
                                    },
                                    new EngineObject
                                    {
                                        Name="Excaliber Winkleford",
                                    }
                                }
                            }
                        }
                    }
                }
            };
        }

        #endregion // GetEngineConfiguration
    }
}