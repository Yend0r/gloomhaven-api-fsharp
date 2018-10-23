namespace GloomChars.Core

module GameData = 
    open System
    open GloomChars.Common
    open FSharpPlus
    
    // Game data is hard coded here. 
    // It will only change if the game gets an update.
    //
    // I had to manufacture id's for the perks so that if 
    // there are perk changes, then the toons will not be affected.
    // I considered putting the data into a db but I also want to make
    // some standalone javascript (reasonml?) apps that use local storage to 
    // store the toons, so db wouldn't work there and it wouldn't be 
    // possible to transfer toon between apps. So hard-coded data works best.

    let modCard action dmg (drawAction : DrawAction) reshuffle = 
        { 
            DrawAnother = drawAction=Draw
            Reshuffle = reshuffle
            Action = action
            Damage = dmg
        }

    let makePerkCard numCards card = 
        { 
            NumCards = numCards
            Card = card
        }

    let private Push amnt = Push (PushAmount amnt)
    let private Pull amnt = Pull (PullAmount amnt)
    let private Pierce amnt = Pierce (PierceAmount amnt)
    let private Heal amnt = Heal (HealAmount amnt)
    let private Shield amnt = Shield (ShieldAmount amnt)

    let private remove numCards action dmg draw = 
        RemoveCard (makePerkCard numCards (modCard action dmg draw false)) 

    let private add numCards action dmg draw = 
        AddCard (makePerkCard numCards (modCard action dmg draw false))

    let private makePerk id qty actions = 
        { 
            Id = id
            Quantity = qty
            Actions = actions
        }

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
                    makePerk "brt01" 1 [remove 2 Damage -1 NoDraw]                        
                    makePerk "brt02" 1 [remove 1 Damage -1 NoDraw; add 1 Damage 1 NoDraw]  
                    makePerk "brt03" 2 [add 2 Damage 1 NoDraw]                            
                    makePerk "brt04" 1 [add 1 Damage 3 NoDraw]                            
                    makePerk "brt05" 2 [add 3 (Push 1) 0 Draw]                            
                    makePerk "brt06" 1 [add 2 (Pierce 3) 0 Draw]                          
                    makePerk "brt07" 2 [add 1 Stun 0 Draw]                                
                    makePerk "brt08" 1 [add 1 Disarm 0 Draw; add 1 Muddle 0 Draw]   
                    makePerk "brt09" 2 [add 1 AddTarget 0 Draw]                           
                    makePerk "brt10" 1 [add 1 (Shield 1) 1 NoDraw]                        
                    makePerk "brt11" 1 [IgnoreItemEffects; add 1 Damage 1 NoDraw]         
                ]

            makeClass Tinkerer "Quatryl Tinkerer" "Cog" true
                [
                    makePerk "tnk01" 2 [remove 2 Damage -1 NoDraw]                        
                    makePerk "tnk02" 1 [remove 1 Damage -2 NoDraw; add 1 Damage 0 NoDraw] 
                    makePerk "tnk03" 1 [add 2 Damage 1 NoDraw]                            
                    makePerk "tnk04" 1 [add 1 Damage 3 NoDraw]                            
                    makePerk "tnk05" 1 [add 2 Fire 0 Draw]                                
                    makePerk "tnk06" 1 [add 3 Muddle 0 Draw]                              
                    makePerk "tnk07" 2 [add 1 Wound 1 NoDraw]                               
                    makePerk "tnk08" 2 [add 1 Immobilise 1 NoDraw]                         
                    makePerk "tnk09" 2 [add 1 (Heal 2) 1 NoDraw]                            
                    makePerk "tnk10" 1 [add 1 AddTarget 0 NoDraw]                         
                    makePerk "tnk11" 1 [IgnoreScenarioEffects]                            
                ]

            makeClass Spellweaver "Orchid Spellweaver" "Spell" true
                [
                    makePerk "spl01" 1 [remove 4 Damage 0 NoDraw]                          
                    makePerk "spl02" 2 [remove 1 Damage -1 NoDraw; add 1 Damage 1 NoDraw]  
                    makePerk "spl03" 2 [add 2 Damage 1 NoDraw]                             
                    makePerk "spl04" 1 [add 1 Stun 0 NoDraw]                               
                    makePerk "spl05" 1 [add 1 Wound 1 NoDraw]                              
                    makePerk "spl06" 1 [add 1 Immobilise 1 NoDraw]                         
                    makePerk "spl07" 1 [add 1 Curse 1 NoDraw]                              
                    makePerk "spl08" 2 [add 1 Fire 2 NoDraw]                               
                    makePerk "spl09" 2 [add 1 Ice 2 NoDraw]                                
                    makePerk "spl10" 1 [add 1 Earth 0 Draw; add 1 Air 0 Draw]              
                    makePerk "spl11" 1 [add 1 Light 0 Draw; add 1 Dark 0 Draw]             
                ]

            makeClass Scoundrel "Human Scoundrel" "ThrowingKnives" true
                [
                    makePerk "scn01" 2 [remove 2 Damage -1 NoDraw]                        
                    makePerk "scn02" 1 [remove 4 Damage 0 NoDraw]                         
                    makePerk "scn03" 1 [remove 1 Damage -2 NoDraw; add 1 Damage 0 NoDraw] 
                    makePerk "scn04" 1 [remove 1 Damage -1 NoDraw; add 1 Damage 1 NoDraw] 
                    makePerk "scn05" 2 [remove 1 Damage 0 NoDraw; add 1 Damage 2 NoDraw]  
                    makePerk "scn06" 2 [add 2 Damage 1 Draw]                              
                    makePerk "scn07" 1 [add 2 (Pierce 3) 0 Draw]                          
                    makePerk "scn08" 2 [add 2 Poison 0 Draw]                              
                    makePerk "scn09" 1 [add 2 Muddle 0 Draw]                              
                    makePerk "scn10" 1 [add 1 Invisible 0 Draw]                           
                    makePerk "scn11" 1 [IgnoreScenarioEffects]                            
                ]

            makeClass Cragheart "Savvas Cragheart" "Rocks" true
                [
                    makePerk "crg01" 1 [remove 4 Damage 0 NoDraw]                          
                    makePerk "crg02" 3 [remove 1 Damage -1 NoDraw; add 1 Damage 1 NoDraw]  
                    makePerk "crg03" 1 [add 1 Damage -2 NoDraw; add 2 Damage 2 NoDraw]     
                    makePerk "crg04" 2 [add 1 Immobilise 1 NoDraw]                         
                    makePerk "crg05" 2 [add 1 Muddle 2 NoDraw]                             
                    makePerk "crg06" 1 [add 2 (Push 2) 0 Draw]                             
                    makePerk "crg07" 2 [add 2 Air 0 Draw]                                  
                    makePerk "crg08" 1 [add 2 Earth 0 Draw]                                
                    makePerk "crg09" 1 [IgnoreItemEffects]                                 
                    makePerk "crg10" 1 [IgnoreScenarioEffects]                             
                ]

            makeClass Mindthief "Vermling Mindthief" "Brain" true
                [
                    makePerk "mnd01" 2 [remove 2 Damage -1 NoDraw]                         
                    makePerk "mnd02" 1 [remove 4 Damage 0 NoDraw]                          
                    makePerk "mnd03" 1 [remove 2 Damage 1 NoDraw; add 2 Damage 2 NoDraw]   
                    makePerk "mnd04" 1 [remove 1 Damage -2 NoDraw; add 1 Damage 0 NoDraw]  
                    makePerk "mnd05" 2 [add 1 Ice 2 NoDraw]                                
                    makePerk "mnd06" 2 [add 2 Damage 1 Draw]                               
                    makePerk "mnd07" 2 [add 3 (Pull 1) 0 Draw]                             
                    makePerk "mnd08" 1 [add 3 Muddle 0 Draw]                               
                    makePerk "mnd09" 1 [add 2 Immobilise 0 Draw]                           
                    makePerk "mnd10" 1 [add 1 Stun 0 Draw]                                 
                    makePerk "mnd11" 1 [add 1 Disarm 0 Draw; add 1 Muddle 0 Draw]          
                    makePerk "mnd12" 1 [IgnoreScenarioEffects]                             
                ]

            makeClass Sunkeeper "Valrath Sunkeeper" "Sun" false
                [
                    makePerk "sun01" 2 [remove 2 Damage -1 NoDraw]                         
                    makePerk "sun02" 1 [remove 4 Damage 0 NoDraw]                          
                    makePerk "sun03" 1 [remove 1 Damage -2 NoDraw; add 1 Damage 0 NoDraw]  
                    makePerk "sun04" 1 [remove 1 Damage 0 NoDraw; add 1 Damage 2 NoDraw]   
                    makePerk "sun05" 2 [add 2 Damage 1 NoDraw]                             
                    makePerk "sun06" 2 [add 2 (Heal 1) 0 Draw]                             
                    makePerk "sun07" 1 [add 1 Stun 0 Draw]                                 
                    makePerk "sun08" 2 [add 2 Light 0 Draw]                                
                    makePerk "sun09" 1 [add 1 (Shield 1) 0 Draw]                           
                    makePerk "sun10" 1 [IgnoreItemEffects; add 2 Damage 1 NoDraw]          
                    makePerk "sun11" 1 [IgnoreScenarioEffects]                             
                ]

            makeClass Quartermaster "Valrath Quartermaster" "TripleArrow" false
                [
                    makePerk "qrt01" 2 [remove 2 Damage -1 NoDraw]                        
                    makePerk "qrt02" 1 [remove 4 Damage 0 NoDraw]                         
                    makePerk "qrt03" 2 [remove 1 Damage 0 NoDraw; add 1 Damage 2 NoDraw]  
                    makePerk "qrt04" 2 [add 2 Damage 1 Draw]                              
                    makePerk "qrt05" 1 [add 3 Muddle 0 Draw]                              
                    makePerk "qrt06" 1 [add 2 (Pierce 3) 0 Draw]                          
                    makePerk "qrt07" 1 [add 1 Stun 0 Draw]                                
                    makePerk "qrt08" 1 [add 1 AddTarget 0 Draw]                           
                    makePerk "qrt09" 3 [add 1 RefreshItem 0 NoDraw]                       
                    makePerk "qrt10" 1 [IgnoreItemEffects; add 2 Damage 1 NoDraw]         
                ]

            makeClass Summoner "Aesther Summoner" "Circles" false
                [
                    makePerk "sum01" 1 [remove 2 Damage -1 NoDraw]                         
                    makePerk "sum02" 1 [remove 1 Damage -2 NoDraw; add 1 Damage 0 NoDraw]  
                    makePerk "sum03" 3 [remove 1 Damage -1 NoDraw; add 1 Damage 1 NoDraw]  
                    makePerk "sum04" 2 [add 1 Damage 2 NoDraw]                             
                    makePerk "sum05" 1 [add 2 Wound 0 Draw]                                
                    makePerk "sum06" 1 [add 2 Poison 0 Draw]                               
                    makePerk "sum07" 3 [add 2 (Heal 1) 0 Draw]                             
                    makePerk "sum08" 1 [add 1 Fire 0 Draw; add 1 Air 0 Draw]               
                    makePerk "sum09" 1 [add 1 Dark 0 Draw; add 1 Earth 0 Draw]             
                    makePerk "sum10" 1 [IgnoreItemEffects; add 2 Damage 1 NoDraw]          
                ]

            makeClass Nightshroud "Aesther Nightshroud" "Eclipse" false
                [
                    makePerk "ngt01" 2 [remove 2 Damage -1 NoDraw]                     
                    makePerk "ngt02" 1 [remove 4 Damage 0 NoDraw]                      
                    makePerk "ngt03" 2 [add 1 Dark -1 NoDraw]                          
                    makePerk "ngt04" 2 [remove 1 Dark -1 NoDraw; add 1 Dark 1 NoDraw]  
                    makePerk "ngt05" 2 [add 1 Invisible 1 NoDraw]                      
                    makePerk "ngt06" 2 [add 3 Muddle 0 Draw]                           
                    makePerk "ngt07" 1 [add 2 (Heal 1) 0 Draw]                         
                    makePerk "ngt08" 1 [add 2 Curse 0 Draw]                            
                    makePerk "ngt09" 1 [add 1 AddTarget 0 Draw]                        
                    makePerk "ngt10" 1 [IgnoreItemEffects; add 2 Damage 1 NoDraw]      
                ]

            makeClass Plagueherald "Harrower Plagueherald" "Cthulthu" false
                [
                    makePerk "plg01" 1 [remove 1 Damage -2 NoDraw; add 1 Damage 0 NoDraw] 
                    makePerk "plg02" 2 [remove 1 Damage -1 NoDraw; add 1 Damage 1 NoDraw] 
                    makePerk "plg03" 2 [remove 1 Damage 0 NoDraw; add 1 Damage 2 NoDraw]  
                    makePerk "plg04" 1 [add 2 Damage 1 NoDraw]                            
                    makePerk "plg05" 3 [add 1 Air 1 NoDraw]                               
                    makePerk "plg06" 1 [add 3 Poison 0 Draw]                              
                    makePerk "plg07" 1 [add 2 Curse 0 Draw]                               
                    makePerk "plg08" 1 [add 2 Immobilise 0 Draw]                          
                    makePerk "plg09" 2 [add 1 Stun 0 Draw]                                
                    makePerk "plg10" 1 [IgnoreScenarioEffects; add 1 Damage 1 NoDraw]         
                ]

            makeClass Berserker "Inox Berserker" "Lightning" false
                [
                    makePerk "brs01" 1 [remove 2 Damage -1 NoDraw]                        
                    makePerk "brs02" 1 [remove 4 Damage 0 NoDraw]                         
                    makePerk "brs03" 2 [remove 1 Damage -1 NoDraw; add 1 Damage 1 NoDraw] 
                    makePerk "brs04" 2 [remove 1 Damage 0 NoDraw; add 1 Damage 2 Draw]    
                    makePerk "brs05" 2 [add 2 Wound 0 Draw]                               
                    makePerk "brs06" 2 [add 1 Stun 0 Draw]                                
                    makePerk "brs07" 1 [add 1 Disarm 1 Draw]                              
                    makePerk "brs08" 1 [add 2 (Heal 1) 0 Draw]                            
                    makePerk "brs09" 2 [add 1 Fire 2 NoDraw]                              
                    makePerk "brs10" 1 [IgnoreItemEffects]                                
                ]

            makeClass Soothsinger "Quatryl Soothsinger" "MusicNote" false
                [
                    makePerk "sth01" 2 [remove 2 Damage -1 NoDraw]                          
                    makePerk "sth02" 1 [remove 1 Damage -2 NoDraw]                          
                    makePerk "sth03" 2 [remove 2 Damage 1 NoDraw; add 1 Damage 4 NoDraw]    
                    makePerk "sth04" 1 [remove 1 Damage 0 NoDraw; add 1 Immobilise 1 NoDraw]
                    makePerk "sth05" 1 [remove 1 Damage 0 NoDraw; add 1 Disarm 1 NoDraw]    
                    makePerk "sth06" 1 [remove 1 Damage 0 NoDraw; add 1 Wound 2 NoDraw]     
                    makePerk "sth07" 1 [remove 1 Damage 0 NoDraw; add 1 Poison 2 NoDraw]    
                    makePerk "sth08" 1 [remove 1 Damage 0 NoDraw; add 1 Curse 2 NoDraw]     
                    makePerk "sth09" 1 [remove 1 Damage 0 NoDraw; add 1 Muddle 3 NoDraw]    
                    makePerk "sth10" 1 [remove 1 Damage -1 NoDraw; add 1 Stun 0 NoDraw]     
                    makePerk "sth11" 1 [add 3 Damage 1 Draw]                                
                    makePerk "sth12" 1 [add 2 Curse 0 Draw]                                   
                ]

            makeClass Doomstalker "Orchid Doomstalker" "Mask" false
                [
                    makePerk "dms01" 2 [remove 2 Damage -1 NoDraw]                         
                    makePerk "dms02" 3 [remove 2 Damage 0 NoDraw; add 2 Damage 1 NoDraw]   
                    makePerk "dms03" 2 [add 2 Damage 1 Draw]                             
                    makePerk "dms04" 1 [add 1 Muddle 2 NoDraw]                             
                    makePerk "dms05" 1 [add 1 Poison 1 NoDraw]                             
                    makePerk "dms06" 1 [add 1 Wound 1 NoDraw]                              
                    makePerk "dms07" 1 [add 1 Immobilise 1 NoDraw]                         
                    makePerk "dms08" 1 [add 1 Stun 0 NoDraw]                               
                    makePerk "dms09" 2 [add 1 AddTarget 0 Draw]                            
                    makePerk "dms10" 1 [IgnoreScenarioEffects]                               
                ]

            makeClass Sawbones "Human Sawbones" "Saw" false
                [
                    makePerk "saw01" 2 [remove 2 Damage -1 NoDraw]                       
                    makePerk "saw02" 1 [remove 4 Damage 0 NoDraw]                        
                    makePerk "saw03" 2 [remove 1 Damage 0 NoDraw; add 1 Damage 2 NoDraw] 
                    makePerk "saw04" 2 [add 1 Damage 2 Draw]                             
                    makePerk "saw05" 2 [add 1 Immobilise 1 NoDraw]                       
                    makePerk "saw06" 2 [add 2 Wound 0 Draw]                              
                    makePerk "saw07" 1 [add 1 Stun 0 Draw]                               
                    makePerk "saw08" 1 [add 1 (Heal 3) 0 Draw]                           
                    makePerk "saw09" 1 [add 1 RefreshItem 0 NoDraw]                      
                ]

            makeClass Elementalist "Savvas Elementalist" "Triangle" false
                [
                    makePerk "elm01" 2 [remove 2 Damage -1 NoDraw]                        
                    makePerk "elm02" 1 [remove 1 Damage -1 NoDraw; add 1 Damage 1 NoDraw] 
                    makePerk "elm03" 2 [remove 1 Damage 0 NoDraw; add 1 Damage 2 NoDraw]  
                    makePerk "elm04" 1 [add 3 Fire 0 NoDraw]                              
                    makePerk "elm05" 1 [add 3 Ice 0 NoDraw]                               
                    makePerk "elm06" 1 [add 3 Air 0 NoDraw]                               
                    makePerk "elm07" 1 [add 3 Earth 0 NoDraw]                         
                    makePerk "elm08" 1 [remove 2 Damage 0 NoDraw; add 1 Fire 0 NoDraw; add 1 Earth 0 NoDraw] 
                    makePerk "elm09" 1 [remove 2 Damage 0 NoDraw; add 1 Ice 0 NoDraw; add 1 Air 0  NoDraw] 
                    makePerk "elm10" 1 [add 2 (Push 1) 1 NoDraw]          
                    makePerk "elm11" 1 [add 1 Wound 1 NoDraw]             
                    makePerk "elm12" 1 [add 1 Stun 0 NoDraw]              
                    makePerk "elm13" 1 [add 1 AddTarget 0 NoDraw]         
                ]

            makeClass BeastTyrant "Vermling Beast Tyrant" "TwoMinis" false
                [
                    makePerk "bst01" 1 [remove 2 Damage -1 NoDraw]                          
                    makePerk "bst02" 3 [remove 1 Damage -1 NoDraw; add 1 Damage 1 NoDraw]   
                    makePerk "bst03" 2 [remove 1 Damage 0 NoDraw; add 1 Damage 2 NoDraw]    
                    makePerk "bst04" 2 [add 1 Wound 1 NoDraw]                               
                    makePerk "bst05" 2 [add 1 Immobilise 1 NoDraw]                          
                    makePerk "bst06" 3 [add 2 (Heal 1) 0 Draw]                            
                    makePerk "bst07" 1 [add 2 Earth 0 Draw]                                 
                    makePerk "bst08" 1 [IgnoreScenarioEffects]                              
                ]
        ]

    let gloomClass (name : GloomClassName) = 
        gloomClasses 
        |> List.find (fun c -> c.ClassName = name)

    let getGloomClass className : GloomClass option = 
        className
        |> GloomClassName.FromString 
        |> map gloomClass

(*
module CSharpCodeGen = 

    let private modCardText (modCard : ModifierCard) = 
        match modCard.Action with
        | Miss -> "Miss"
        | Damage -> "Damage"
        | MultiplyDamage amount -> 
            let (DamageMultiplier value) = amount
            sprintf "MultiplyDamage, %i" value
        | Disarm -> "Disarm"
        | Stun -> "Stun"
        | Poison -> "Poison" 
        | Wound  -> "Wound" 
        | Muddle -> "Muddle"
        | AddTarget -> "AddTarget"
        | Immobilise -> "Immobilise"
        | Invisible -> "Invisible"
        | Fire -> "Fire"
        | Ice -> "Ice"
        | Light -> "Light"
        | Air -> "Air"
        | Dark -> "Dark"
        | Earth -> "Earth"
        | Curse -> "Curse"
        | RefreshItem -> "RefreshItem"
        | Push amount ->
            let (PushAmount value) = amount
            sprintf "Push, %i" value
        | Pull amount ->
            let (PullAmount value) = amount
            sprintf "Pull, %i" value
        | Pierce amount ->
            let (PierceAmount value) = amount
            sprintf "Pierce, %i" value
        | Heal amount ->
            let (HealAmount value) = amount
            sprintf "Heal, %i" value
        | Shield amount ->
            let (ShieldAmount value) = amount
            sprintf "Shield, %i" value
            

    let addCardText (p : PerkCardAction) = 
        sprintf "AddCard(%i, CardAction.%s, %i, %b)" p.NumCards (modCardText p.Card) p.Card.Damage p.Card.DrawAnother

    let removeCardText (p : PerkCardAction) = 
        sprintf "RemoveCard(%i, CardAction.%s, %i, %b)" p.NumCards (modCardText p.Card) p.Card.Damage p.Card.DrawAnother
        
    let getActionText (pAction : PerkAction) = 
        match pAction with
        | AddCard cardAction -> addCardText cardAction
        | RemoveCard cardAction -> removeCardText cardAction
        | IgnoreScenarioEffects -> "IgnoreScenario()" 
        | IgnoreItemEffects -> "IgnoreItems()" 

    let perkText (p : Perk) = 
        let actiontext = p.Actions |> List.map getActionText |> String.concat "." 

        sprintf """
                        .WithPerk(Perk.Create("%s", %i).%s)""" p.Id p.Quantity actiontext

    let classText (g : GloomClass) =
        let cName = g.ClassName.ToString()
        let create = sprintf """GloomClass.Create(GloomClassName.%s, "%s", "%s", %b)""" cName g.Name g.Symbol g.IsStarting
        let perks = g.Perks |> List.map perkText |> String.concat ""

        sprintf """
        private GloomClass %s()
        {
            var gClass = 
                %s 
                        %s;
            return gClass;
        }
        """ cName create perks


    let classesText () = 
        GameData.gloomClasses
        |> List.map classText
        |> String.concat "\n"

        *)