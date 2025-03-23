using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Command_parsing.Command_parts;

namespace Command_parsing
{
    public static class Command_model_builder
    {
        //TODO need to support some kind of version ranges here as well, same in entity validator
        public static Command_model Get_data(int version)
        {
            Command_choice_part get_strain;
            Command_choice_part merge_strain;
            Command_choice_part remove_strain;
            Command_choice_part modify_strain;

            Command_choice modify_source;
            Command_choice modify_types;

            if (version < Versions.Get_own_version("1.15"))
            {
                modify_source = new(new Command_choice_part("from", new Command_choice(new Command_choice_part("block", new Command_pos(), new Command_text(Parser_creator.Validate_nbt_path, true)), new Command_choice_part("entity", new Command_entity(false, true, false), new Command_text(Parser_creator.Validate_nbt_path, true)))), new Command_choice_part("value", new Command_text(Parser_creator.Validate_nbt_value)));
                modify_types = new(new Command_choice_part("append", modify_source), new Command_choice_part("insert", new Command_int(), modify_source), new Command_choice_part("merge"), new Command_choice_part("prepend", modify_source), new Command_choice_part("set", modify_source));


                get_strain = new("get", new Command_choice(new Command_choice_part("block", new Command_pos(), new Command_text(Parser_creator.Validate_nbt_path), new Command_float(true)), new Command_choice_part("entity", new Command_entity(false, true, false), new Command_text(Parser_creator.Validate_nbt_path), new Command_float(true))));
                merge_strain = new("merge", new Command_choice(new Command_choice_part("block", new Command_pos(), new Command_text(Parser_creator.Validate_nbt)), new Command_choice_part("entity", new Command_entity(false, true, false), new Command_text(Parser_creator.Validate_nbt))));
                remove_strain = new("remove", new Command_choice(new Command_choice_part("block", new Command_pos(), new Command_text(Parser_creator.Validate_nbt_path)), new Command_choice_part("entity", new Command_entity(false, true, false), new Command_text(Parser_creator.Validate_nbt_path))));
                modify_strain = new("modify", new Command_choice(new Command_choice_part("block", new Command_pos(), new Command_text(Parser_creator.Validate_nbt_path), modify_types), new Command_choice_part("entity", new Command_entity(false, true, false), new Command_text(Parser_creator.Validate_nbt_path), modify_types)));
            }
            else
            {
                //Change here is storage, both as source and destination
                modify_source = new(new Command_choice_part("from", new Command_choice(new Command_choice_part("storage", new Command_text(), new Command_text(Parser_creator.Validate_nbt_path)), new Command_choice_part("block", new Command_pos(), new Command_text(Parser_creator.Validate_nbt_path, true)), new Command_choice_part("entity", new Command_entity(false, true, false), new Command_text(Parser_creator.Validate_nbt_path, true)))), new Command_choice_part("value", new Command_text(Parser_creator.Validate_nbt_value)));
                modify_types = new(new Command_choice_part("append", modify_source), new Command_choice_part("insert", new Command_int(), modify_source), new Command_choice_part("merge"), new Command_choice_part("prepend", modify_source), new Command_choice_part("set", modify_source));


                get_strain = new("get", new Command_choice(new Command_choice_part("storage", new Command_text(), new Command_text(Parser_creator.Validate_nbt_path), new Command_float(true)), new Command_choice_part("block", new Command_pos(), new Command_text(Parser_creator.Validate_nbt_path), new Command_float(true)), new Command_choice_part("entity", new Command_entity(false, true, false), new Command_text(Parser_creator.Validate_nbt_path), new Command_float(true))));
                merge_strain = new("merge", new Command_choice(new Command_choice_part("storage", new Command_text(), new Command_text(Parser_creator.Validate_nbt)), new Command_choice_part("block", new Command_pos(), new Command_text(Parser_creator.Validate_nbt)), new Command_choice_part("entity", new Command_entity(false, true, false), new Command_text(Parser_creator.Validate_nbt))));
                remove_strain = new("remove", new Command_choice(new Command_choice_part("storage", new Command_text(), new Command_text(Parser_creator.Validate_nbt_path), new Command_float(true)), new Command_choice_part("block", new Command_pos(), new Command_text(Parser_creator.Validate_nbt_path)), new Command_choice_part("entity", new Command_entity(false, true, false), new Command_text(Parser_creator.Validate_nbt_path))));
                modify_strain = new("modify", new Command_choice(new Command_choice_part("storage", new Command_text(), new Command_text(Parser_creator.Validate_nbt_path), modify_types), new Command_choice_part("block", new Command_pos(), new Command_text(Parser_creator.Validate_nbt_path), modify_types), new Command_choice_part("entity", new Command_entity(false, true, false), new Command_text(Parser_creator.Validate_nbt_path), modify_types)));
            }


            if (version < Versions.Get_own_version("1.14"))
            {
                return new Command_model(new Command_name("data"), new Command_choice(get_strain, merge_strain, remove_strain));
            }

            
            
            return new Command_model(new Command_name("data"), new Command_choice(get_strain, merge_strain, remove_strain, modify_strain));
        }

        public static Command_model Get_execute(int version)
        {
            Command_choice_part align_strain = new("align", new Command_text("ALIGNMENT"), new Command_execute_stop());
            Command_choice_part anchored_strain = new("anchored", new Command_choice(new string[] { "eyes", "feet" }), new Command_execute_stop());
            Command_choice_part as_strain = new("as", new Command_entity(), new Command_execute_stop());
            Command_choice_part at_strain = new("at", new Command_entity(), new Command_execute_stop());
            Command_choice_part facing_strand = new("facing", new Command_choice(new Command_choice_part(new Command_pos(/*TODO this either the pos or the part with the name*/), new Command_execute_stop()), new Command_choice_part("entity", new Command_entity(), new Command_choice(new string[] { "eyes", "feet" }), new Command_execute_stop())));
            Command_choice_part in_strand = new("in", new Command_text("DIMENSION"), new Command_execute_stop());
            Command_choice_part positioned_strand = new("positioned", new Command_choice(new Command_choice_part(new Command_pos(), new Command_execute_stop()), new Command_choice_part("as", new Command_entity(), new Command_execute_stop())));
            Command_choice_part rotated_strand = new("rotated", new Command_choice(new string[] { /*TODO TWO coords as well*/ "as" }), new Command_entity(), new Command_execute_stop());

            Command_choice_part store_strain;
            Command_choice_part unless_strain;
            Command_choice_part if_strain;

            //store changes
            if(version < Versions.Get_own_version("1.15"))
            {
                store_strain = new("store", new Command_choice(new string[] { "result", "success" }), new Command_choice(new Command_choice_part("block", new Command_pos(), new Command_text(Parser_creator.Validate_nbt_path), new Command_text("NBT_SIZE"), new Command_float(), new Command_execute_stop()), new Command_choice_part("bossbar", new Command_text(), new Command_choice(new string[] { "max", "value" }), new Command_execute_stop()), new Command_choice_part("entity", new Command_entity(false, true, false), new Command_text(Parser_creator.Validate_nbt_path), new Command_text("NBT_SIZE"), new Command_float(), new Command_execute_stop()), new Command_choice_part("score", new Command_entity(), new Command_text(), new Command_execute_stop())));
            }
            else
            {
               store_strain = new("store", new Command_choice(new string[] { "result", "success" }), new Command_choice(new Command_choice_part("storage", new Command_text(), new Command_text(Parser_creator.Validate_nbt_path), new Command_text("NBT_SIZE"), new Command_float(), new Command_execute_stop()), new Command_choice_part("block", new Command_pos(), new Command_text(Parser_creator.Validate_nbt_path), new Command_text("NBT_SIZE"), new Command_float(), new Command_execute_stop()), new Command_choice_part("bossbar", new Command_text(), new Command_choice(new string[] { "max", "value" }), new Command_execute_stop()), new Command_choice_part("entity", new Command_entity(false, true, false), new Command_text(Parser_creator.Validate_nbt_path), new Command_text("NBT_SIZE"), new Command_float(), new Command_execute_stop()), new Command_choice_part("score", new Command_entity(), new Command_text(), new Command_execute_stop())));
            }

            //If/unless changes
            if (version < Versions.Get_own_version("1.14"))
            {
                unless_strain = new("unless", new Command_choice(new Command_choice_part("block", new Command_pos(), new Command_text(Parser_creator.Validate_blocks_tags_data_nbt), new Command_execute_stop()), new Command_choice_part("blocks", new Command_pos(), new Command_pos(), new Command_pos(), new Command_choice(new string[] { "all", "masked" }), new Command_execute_stop()), new Command_choice_part("entity", new Command_entity(), new Command_execute_stop()), new Command_choice_part("score", new Command_entity(), new Command_text(), new Command_choice(/*TODO range*/new Command_choice_part("matches", new Command_text(), new Command_execute_stop()), new Command_choice_part(new string[] { "<=", "<", "=", ">", ">=" }, new Command_entity(), new Command_text(), new Command_execute_stop())))));
                if_strain =         new("if", new Command_choice(new Command_choice_part("block", new Command_pos(), new Command_text(Parser_creator.Validate_blocks_tags_data_nbt), new Command_execute_stop()), new Command_choice_part("blocks", new Command_pos(), new Command_pos(), new Command_pos(), new Command_choice(new string[] { "all", "masked" }), new Command_execute_stop()), new Command_choice_part("entity", new Command_entity(), new Command_execute_stop()), new Command_choice_part("score", new Command_entity(), new Command_text(), new Command_choice(/*TODO range*/new Command_choice_part("matches", new Command_text(), new Command_execute_stop()), new Command_choice_part(new string[] { "<=", "<", "=", ">", ">=" }, new Command_entity(), new Command_text(), new Command_execute_stop())))));

            }
            else if(version < Versions.Get_own_version("1.15"))
            {
                unless_strain = new("unless", new Command_choice(new Command_choice_part("data", new Command_choice(new Command_choice_part("entity", new Command_entity(false, true, false), new Command_text(Parser_creator.Validate_nbt_path), new Command_execute_stop()), new Command_choice_part("block", new Command_pos(), new Command_text(Parser_creator.Validate_nbt_path), new Command_execute_stop()))), new Command_choice_part("block", new Command_pos(), new Command_text(Parser_creator.Validate_blocks_tags_data_nbt), new Command_execute_stop()), new Command_choice_part("blocks", new Command_pos(), new Command_pos(), new Command_pos(), new Command_choice(new string[] { "all", "masked" }), new Command_execute_stop()), new Command_choice_part("entity", new Command_entity(), new Command_execute_stop()), new Command_choice_part("score", new Command_entity(), new Command_text(), new Command_choice(/*TODO range*/new Command_choice_part("matches", new Command_text(), new Command_execute_stop()), new Command_choice_part(new string[] { "<=", "<", "=", ">", ">=" }, new Command_entity(), new Command_text(), new Command_execute_stop())))));
                if_strain =         new("if", new Command_choice(new Command_choice_part("data", new Command_choice(new Command_choice_part("entity", new Command_entity(false, true, false), new Command_text(Parser_creator.Validate_nbt_path), new Command_execute_stop()), new Command_choice_part("block", new Command_pos(), new Command_text(Parser_creator.Validate_nbt_path), new Command_execute_stop()))), new Command_choice_part("block", new Command_pos(), new Command_text(Parser_creator.Validate_blocks_tags_data_nbt), new Command_execute_stop()), new Command_choice_part("blocks", new Command_pos(), new Command_pos(), new Command_pos(), new Command_choice(new string[] { "all", "masked" }), new Command_execute_stop()), new Command_choice_part("entity", new Command_entity(), new Command_execute_stop()), new Command_choice_part("score", new Command_entity(), new Command_text(), new Command_choice(/*TODO range*/new Command_choice_part("matches", new Command_text(), new Command_execute_stop()), new Command_choice_part(new string[] { "<=", "<", "=", ">", ">=" }, new Command_entity(), new Command_text(), new Command_execute_stop())))));
            }
            else
            {
                unless_strain = new("unless", new Command_choice(new Command_choice_part("predicate", new Command_text(), new Command_execute_stop()), new Command_choice_part("data", new Command_choice(new Command_choice_part("storage", new Command_text(), new Command_text(Parser_creator.Validate_nbt_path), new Command_execute_stop()) , new Command_choice_part("entity", new Command_entity(false, true, false), new Command_text(Parser_creator.Validate_nbt_path), new Command_execute_stop()), new Command_choice_part("block", new Command_pos(), new Command_text(Parser_creator.Validate_nbt_path), new Command_execute_stop()))), new Command_choice_part("block", new Command_pos(), new Command_text(Parser_creator.Validate_blocks_tags_data_nbt), new Command_execute_stop()), new Command_choice_part("blocks", new Command_pos(), new Command_pos(), new Command_pos(), new Command_choice(new string[] { "all", "masked" }), new Command_execute_stop()), new Command_choice_part("entity", new Command_entity(), new Command_execute_stop()), new Command_choice_part("score", new Command_entity(), new Command_text(), new Command_choice(/*TODO range*/new Command_choice_part("matches", new Command_text(), new Command_execute_stop()), new Command_choice_part(new string[] { "<=", "<", "=", ">", ">=" }, new Command_entity(), new Command_text(), new Command_execute_stop())))));
                if_strain =         new("if", new Command_choice(new Command_choice_part("predicate", new Command_text(), new Command_execute_stop()), new Command_choice_part("data", new Command_choice(new Command_choice_part("storage", new Command_text(), new Command_text(Parser_creator.Validate_nbt_path), new Command_execute_stop()), new Command_choice_part("entity", new Command_entity(false, true, false), new Command_text(Parser_creator.Validate_nbt_path), new Command_execute_stop()), new Command_choice_part("block", new Command_pos(), new Command_text(Parser_creator.Validate_nbt_path), new Command_execute_stop()))), new Command_choice_part("block", new Command_pos(), new Command_text(Parser_creator.Validate_blocks_tags_data_nbt), new Command_execute_stop()), new Command_choice_part("blocks", new Command_pos(), new Command_pos(), new Command_pos(), new Command_choice(new string[] { "all", "masked" }), new Command_execute_stop()), new Command_choice_part("entity", new Command_entity(), new Command_execute_stop()), new Command_choice_part("score", new Command_entity(), new Command_text(), new Command_choice(/*TODO range*/new Command_choice_part("matches", new Command_text(), new Command_execute_stop()), new Command_choice_part(new string[] { "<=", "<", "=", ">", ">=" }, new Command_entity(), new Command_text(), new Command_execute_stop())))));

            }

            return new Command_model(new Command_name("execute"), new Command_choice(align_strain, anchored_strain, as_strain, at_strain, facing_strand, in_strand, positioned_strand, rotated_strand, unless_strain, if_strain, store_strain));
        }
    }
}
