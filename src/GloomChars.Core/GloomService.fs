namespace GloomChars.Core

module GloomService = 
    open DeckService 

    // Some helper methods...

    let makePerkCard numCards card = 
        { NumCards = numCards; Card = card; }

    let private Push amnt = Push (PushAmount amnt)
    let private Pull amnt = Pull (PullAmount amnt)
    let private Pierce amnt = Pierce (PierceAmount amnt)
    let private Heal amnt = Heal (HealAmount amnt)
    let private Shield amnt = Shield (ShieldAmount amnt)

    let private remove numCards action dmg draw = 
        RemoveCard (makePerkCard numCards (modCard action dmg draw)) 

    let private add numCards action dmg draw = 
        AddCard (makePerkCard numCards (modCard action dmg draw))

    let private makePerk qty actions = 
        { Quantity = qty; Actions = actions; }

    let private makeClass className name symbol isStarting perks = 
        { 
            ClassName = className
            Name = name
            Symbol = symbol
            IsStarting = isStarting
            Perks = perks 
        }

    // This will get cached after the first call 
    let gloomClasses : GloomClass list = 
        [
            makeClass Brute "Inox Brute" "Horns" true
                [
                    makePerk 1 [remove 2 Damage -1 NoDraw]                        
                    makePerk 1 [remove 1 Damage -1 NoDraw; add 1 Damage 1 NoDraw]  
                    makePerk 2 [add 2 Damage 1 NoDraw]                            
                    makePerk 1 [add 1 Damage 3 NoDraw]                            
                    makePerk 2 [add 3 (Push 1) 0 Draw]                            
                    makePerk 1 [add 2 (Pierce 3) 0 Draw]                          
                    makePerk 2 [add 1 Stun 0 Draw]                                
                    makePerk 1 [add 1 Disarm 0 Draw; add 1 Muddle 0 Draw]   
                    makePerk 2 [add 1 AddTarget 0 Draw]                           
                    makePerk 1 [add 1 (Shield 1) 1 NoDraw]                        
                    makePerk 1 [IgnoreItemEffects; add 1 Damage 1 NoDraw]         
                ]

            makeClass Tinkerer "Quatryl Tinkerer" "Cog" true
                [
                    makePerk 2 [remove 2 Damage -1 NoDraw]                        
                    makePerk 1 [remove 1 Damage -2 NoDraw; add 1 Damage 0 NoDraw] 
                    makePerk 1 [add 2 Damage 1 NoDraw]                            
                    makePerk 1 [add 1 Damage 3 NoDraw]                            
                    makePerk 1 [add 2 Fire 0 Draw]                                
                    makePerk 1 [add 3 Muddle 0 Draw]                              
                    makePerk 2 [add 1 Wound 1 NoDraw]                               
                    makePerk 2 [add 1 Immobilise 1 NoDraw]                         
                    makePerk 2 [add 1 (Heal 2) 1 NoDraw]                            
                    makePerk 1 [add 1 AddTarget 0 NoDraw]                         
                    makePerk 1 [IgnoreScenarioEffects]                            
                ]

            makeClass Spellweaver "Orchid Spellweaver" "Spell" true
                [
                    makePerk 1 [remove 4 Damage 0 NoDraw]                          
                    makePerk 2 [remove 1 Damage -1 NoDraw; add 1 Damage 1 NoDraw]  
                    makePerk 2 [add 2 Damage 1 NoDraw]                             
                    makePerk 1 [add 1 Stun 0 NoDraw]                               
                    makePerk 1 [add 1 Wound 1 NoDraw]                              
                    makePerk 1 [add 1 Immobilise 1 NoDraw]                         
                    makePerk 1 [add 1 Curse 1 NoDraw]                              
                    makePerk 2 [add 1 Fire 2 NoDraw]                               
                    makePerk 2 [add 1 Ice 2 NoDraw]                                
                    makePerk 1 [add 1 Earth 0 Draw; add 1 Air 0 Draw]              
                    makePerk 1 [add 1 Light 0 Draw; add 1 Dark 0 Draw]             
                ]

            makeClass Scoundrel "Human Scoundrel" "ThrowingKnives" true
                [
                    makePerk 2 [remove 2 Damage -1 NoDraw]                        
                    makePerk 1 [remove 4 Damage 0 NoDraw]                         
                    makePerk 1 [remove 1 Damage -2 NoDraw; add 1 Damage 0 NoDraw] 
                    makePerk 1 [remove 1 Damage -1 NoDraw; add 1 Damage 1 NoDraw] 
                    makePerk 2 [remove 1 Damage 0 NoDraw; add 1 Damage 2 NoDraw]  
                    makePerk 2 [add 2 Damage 1 Draw]                              
                    makePerk 1 [add 2 (Pierce 3) 0 Draw]                          
                    makePerk 2 [add 2 Poison 0 Draw]                              
                    makePerk 1 [add 2 Muddle 0 Draw]                              
                    makePerk 1 [add 1 Invisible 0 Draw]                           
                    makePerk 1 [IgnoreScenarioEffects]                            
                ]

            makeClass Cragheart "Savvas Cragheart" "Rocks" true
                [
                    makePerk 1 [remove 4 Damage 0 NoDraw]                          
                    makePerk 3 [remove 1 Damage -1 NoDraw; add 1 Damage 1 NoDraw]  
                    makePerk 1 [add 1 Damage -2 NoDraw; add 2 Damage 2 NoDraw]     
                    makePerk 2 [add 1 Immobilise 1 NoDraw]                         
                    makePerk 2 [add 1 Muddle 2 NoDraw]                             
                    makePerk 1 [add 2 (Push 2) 0 Draw]                             
                    makePerk 2 [add 2 Air 0 Draw]                                  
                    makePerk 1 [add 2 Earth 0 Draw]                                
                    makePerk 1 [IgnoreItemEffects]                                 
                    makePerk 1 [IgnoreScenarioEffects]                             
                ]

            makeClass Mindthief "Vermling Mindthief" "Brain" true
                [
                    makePerk 2 [remove 2 Damage -1 NoDraw]                         
                    makePerk 1 [remove 4 Damage 0 NoDraw]                          
                    makePerk 1 [remove 2 Damage 1 NoDraw; add 2 Damage 2 NoDraw]   
                    makePerk 1 [remove 1 Damage -2 NoDraw; add 1 Damage 0 NoDraw]  
                    makePerk 2 [add 1 Ice 2 NoDraw]                                
                    makePerk 2 [add 2 Damage 1 Draw]                               
                    makePerk 2 [add 3 (Pull 1) 0 Draw]                             
                    makePerk 1 [add 3 Muddle 0 Draw]                               
                    makePerk 1 [add 2 Immobilise 0 Draw]                           
                    makePerk 1 [add 1 Stun 0 Draw]                                 
                    makePerk 1 [add 1 Disarm 0 Draw; add 1 Muddle 0 Draw]          
                    makePerk 1 [IgnoreScenarioEffects]                             
                ]

            makeClass Sunkeeper "Valrath Sunkeeper" "Sun" false
                [
                    makePerk 2 [remove 2 Damage -1 NoDraw]                         
                    makePerk 1 [remove 4 Damage 0 NoDraw]                          
                    makePerk 1 [remove 1 Damage -2 NoDraw; add 1 Damage 0 NoDraw]  
                    makePerk 1 [remove 1 Damage 0 NoDraw; add 1 Damage 2 NoDraw]   
                    makePerk 2 [add 2 Damage 1 NoDraw]                             
                    makePerk 2 [add 2 (Heal 1) 0 Draw]                             
                    makePerk 1 [add 1 Stun 0 Draw]                                 
                    makePerk 2 [add 2 Light 0 Draw]                                
                    makePerk 1 [add 1 (Shield 1) 0 Draw]                           
                    makePerk 1 [IgnoreItemEffects; add 2 Damage 1 NoDraw]          
                    makePerk 1 [IgnoreScenarioEffects]                             
                ]

            makeClass Quartermaster "Valrath Quartermaster" "TripleArrow" false
                [
                    makePerk 2 [remove 2 Damage -1 NoDraw]                        
                    makePerk 1 [remove 4 Damage 0 NoDraw]                         
                    makePerk 2 [remove 1 Damage 0 NoDraw; add 1 Damage 2 NoDraw]  
                    makePerk 2 [add 2 Damage 1 Draw]                              
                    makePerk 1 [add 3 Muddle 0 Draw]                              
                    makePerk 1 [add 2 (Pierce 3) 0 Draw]                          
                    makePerk 1 [add 1 Stun 0 Draw]                                
                    makePerk 1 [add 1 AddTarget 0 Draw]                           
                    makePerk 3 [add 1 RefreshItem 0 NoDraw]                       
                    makePerk 1 [IgnoreItemEffects; add 2 Damage 1 NoDraw]         
                ]

            makeClass Summoner "Aesther Summoner" "Circles" false
                [
                    makePerk 1 [remove 2 Damage -1 NoDraw]                         
                    makePerk 1 [remove 1 Damage -2 NoDraw; add 1 Damage 0 NoDraw]  
                    makePerk 3 [remove 1 Damage -1 NoDraw; add 1 Damage 1 NoDraw]  
                    makePerk 2 [add 1 Damage 2 NoDraw]                             
                    makePerk 1 [add 2 Wound 0 Draw]                                
                    makePerk 1 [add 2 Poison 0 Draw]                               
                    makePerk 3 [add 2 (Heal 1) 0 Draw]                             
                    makePerk 1 [add 1 Fire 0 Draw; add 1 Air 0 Draw]               
                    makePerk 1 [add 1 Dark 0 Draw; add 1 Earth 0 Draw]             
                    makePerk 1 [IgnoreItemEffects; add 2 Damage 1 NoDraw]          
                ]

            makeClass Nightshroud "Aesther Nightshroud" "Eclipse" false
                [
                    makePerk 2 [remove 2 Damage -1 NoDraw]                     
                    makePerk 1 [remove 4 Damage 0 NoDraw]                      
                    makePerk 2 [add 1 Dark -1 NoDraw]                          
                    makePerk 2 [remove 1 Dark -1 NoDraw; add 1 Dark 1 NoDraw]  
                    makePerk 2 [add 1 Invisible 1 NoDraw]                      
                    makePerk 2 [add 3 Muddle 0 Draw]                           
                    makePerk 1 [add 2 (Heal 1) 0 Draw]                         
                    makePerk 1 [add 2 Curse 0 Draw]                            
                    makePerk 1 [add 1 AddTarget 0 Draw]                        
                    makePerk 1 [IgnoreItemEffects; add 2 Damage 1 NoDraw]      
                ]

            makeClass Plagueherald "Harrower Plagueherald" "Cthulthu" false
                [
                    makePerk 1 [remove 1 Damage -2 NoDraw; add 1 Damage 0 NoDraw] 
                    makePerk 2 [remove 1 Damage -1 NoDraw; add 1 Damage 1 NoDraw] 
                    makePerk 2 [remove 1 Damage 0 NoDraw; add 1 Damage 2 NoDraw]  
                    makePerk 1 [add 2 Damage 1 NoDraw]                            
                    makePerk 3 [add 1 Air 1 NoDraw]                               
                    makePerk 1 [add 3 Poison 0 Draw]                              
                    makePerk 1 [add 2 Curse 0 Draw]                               
                    makePerk 1 [add 2 Immobilise 0 Draw]                          
                    makePerk 2 [add 1 Stun 0 Draw]                                
                    makePerk 1 [IgnoreScenarioEffects; add 1 Damage 1 NoDraw]         
                ]

            makeClass Berserker "Inox Berserker" "Lightning" false
                [
                    makePerk 1 [remove 2 Damage -1 NoDraw]                        
                    makePerk 1 [remove 4 Damage 0 NoDraw]                         
                    makePerk 2 [remove 1 Damage -1 NoDraw; add 1 Damage 1 NoDraw] 
                    makePerk 2 [remove 1 Damage 0 NoDraw; add 1 Damage 2 Draw]    
                    makePerk 2 [add 2 Wound 0 Draw]                               
                    makePerk 2 [add 1 Stun 0 Draw]                                
                    makePerk 1 [add 1 Disarm 1 Draw]                              
                    makePerk 1 [add 2 (Heal 1) 0 Draw]                            
                    makePerk 2 [add 1 Fire 2 NoDraw]                              
                    makePerk 1 [IgnoreItemEffects]                                
                ]

            makeClass Soothsinger "Quatryl Soothsinger" "MusicNote" false
                [
                    makePerk 2 [remove 2 Damage -1 NoDraw]                          
                    makePerk 1 [remove 1 Damage -2 NoDraw]                          
                    makePerk 2 [remove 2 Damage 1 NoDraw; add 1 Damage 4 NoDraw]    
                    makePerk 1 [remove 1 Damage 0 NoDraw; add 1 Immobilise 1 NoDraw]
                    makePerk 1 [remove 1 Damage 0 NoDraw; add 1 Disarm 1 NoDraw]    
                    makePerk 1 [remove 1 Damage 0 NoDraw; add 1 Wound 2 NoDraw]     
                    makePerk 1 [remove 1 Damage 0 NoDraw; add 1 Poison 2 NoDraw]    
                    makePerk 1 [remove 1 Damage 0 NoDraw; add 1 Curse 2 NoDraw]     
                    makePerk 1 [remove 1 Damage 0 NoDraw; add 1 Muddle 3 NoDraw]    
                    makePerk 1 [remove 1 Damage -1 NoDraw; add 1 Stun 0 NoDraw]     
                    makePerk 1 [add 3 Damage 1 Draw]                                
                    makePerk 1 [add 2 Curse 0 Draw]                                   
                ]

            makeClass Doomstalker "Orchid Doomstalker" "Mask" false
                [
                    makePerk 2 [remove 2 Damage -1 NoDraw]                         
                    makePerk 3 [remove 2 Damage 0 NoDraw; add 2 Damage 1 NoDraw]   
                    makePerk 2 [add 2 Damage 1 Draw]                             
                    makePerk 1 [add 1 Muddle 2 NoDraw]                             
                    makePerk 1 [add 1 Poison 1 NoDraw]                             
                    makePerk 1 [add 1 Wound 1 NoDraw]                              
                    makePerk 1 [add 1 Immobilise 1 NoDraw]                         
                    makePerk 1 [add 1 Stun 0 NoDraw]                               
                    makePerk 2 [add 1 AddTarget 0 Draw]                            
                    makePerk 1 [IgnoreScenarioEffects]                               
                ]

            makeClass Sawbones "Human Sawbones" "Saw" false
                [
                    makePerk 2 [remove 2 Damage -1 NoDraw]                       
                    makePerk 1 [remove 4 Damage 0 NoDraw]                        
                    makePerk 2 [remove 1 Damage 0 NoDraw; add 1 Damage 2 NoDraw] 
                    makePerk 2 [add 1 Damage 2 Draw]                             
                    makePerk 2 [add 1 Immobilise 1 NoDraw]                       
                    makePerk 2 [add 2 Wound 0 Draw]                              
                    makePerk 1 [add 1 Stun 0 Draw]                               
                    makePerk 1 [add 1 (Heal 3) 0 Draw]                           
                    makePerk 1 [add 1 RefreshItem 0 NoDraw]                      
                ]

            makeClass Elementalist "Savvas Elementalist" "Triangle" false
                [
                    makePerk 2 [remove 2 Damage -1 NoDraw]                        
                    makePerk 1 [remove 1 Damage -1 NoDraw; add 1 Damage 1 NoDraw] 
                    makePerk 2 [remove 1 Damage 0 NoDraw; add 1 Damage 2 NoDraw]  
                    makePerk 1 [add 3 Fire 0 NoDraw]                              
                    makePerk 1 [add 3 Ice 0 NoDraw]                               
                    makePerk 1 [add 3 Air 0 NoDraw]                               
                    makePerk 1 [add 3 Earth 0 NoDraw]                         
                    makePerk 1 [remove 2 Damage 0 NoDraw; add 1 Fire 0 NoDraw; add 1 Earth 0 NoDraw] 
                    makePerk 1 [remove 2 Damage 0 NoDraw; add 1 Ice 0 NoDraw; add 1 Air 0  NoDraw] 
                    makePerk 1 [add 2 (Push 1) 1 NoDraw]          
                    makePerk 1 [add 1 Wound 1 NoDraw]             
                    makePerk 1 [add 1 Stun 0 NoDraw]              
                    makePerk 1 [add 1 AddTarget 0 NoDraw]         
                ]

            makeClass BeastTyrant "Vermling Beast Tyrant" "TwoMinis" false
                [
                    makePerk 1 [remove 2 Damage -1 NoDraw]                          
                    makePerk 3 [remove 1 Damage -1 NoDraw; add 1 Damage 1 NoDraw]   
                    makePerk 2 [remove 1 Damage 0 NoDraw; add 1 Damage 2 NoDraw]    
                    makePerk 2 [add 1 Wound 1 NoDraw]                               
                    makePerk 2 [add 1 Immobilise 1 NoDraw]                          
                    makePerk 3 [add 2 (Heal 1) 0 Draw]                            
                    makePerk 1 [add 2 Earth 0 Draw]                                 
                    makePerk 1 [IgnoreScenarioEffects]                              
                ]
        ]

    let gloomClass className = 
        gloomClasses 
        |> List.find (fun c -> c.ClassName = className)
