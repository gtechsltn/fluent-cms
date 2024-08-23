import {LookupContainer} from "./LookupContainer";
import {TextInput} from "../../components/inputs/TextInput";
import {TextAreaInput} from "../../components/inputs/TextAreaInput";
import {EditorInput} from "../../components/inputs/EditorInput";
import {NumberInput} from "../../components/inputs/NumberInput";
import {DatetimeInput} from "../../components/inputs/DatetimeInput";
import {DateInput} from "../../components/inputs/DateInput";
import {FileInput} from "../../components/inputs/FileInput";
import {GalleryInput} from "../../components/inputs/GalleryInput";
import {DropDownInput} from "../../components/inputs/DropDownInput";
import {MultiSelectInput} from "../../components/inputs/MultiSelectInput";

export function createInput(props :any) {
    const {type, field} = props.column
    switch (type) {
        case 'text':
            return <TextInput className={'field col-12 md:col-4'} key={field} {...props}/>
        case 'textarea':
            return <TextAreaInput className={'field col-12  md:col-4'} key={field} {...props}/>
        case 'editor':
            return <EditorInput className={'field col-12'} key={field} {...props}/>

        case 'number':
            return <NumberInput className={'field col-12 md:col-4'} key={field} {...props}/>

        case 'datetime':
            return <DatetimeInput className={'field col-12  md:col-4'} key={field} {...props}/>
        case 'date':
            return <DateInput className={'field col-12  md:col-4'} key={field} {...props}/>

        case 'image':
            return <FileInput previewImage className={'field col-12  md:col-4'} key={field} {...props}/>
        case 'gallery':
            return <GalleryInput className={'field col-12  md:col-4'} key={field} {...props}/>
        case 'file':
            return <FileInput download className={'field col-12  md:col-4'} key={field} {...props}/>

        case 'dropdown':
            return <DropDownInput className={'field col-12 md:col-4'} key={field}{...props}/>
        case 'lookup':
            return <LookupContainer className={'field col-12 md:col-4'} key={field}{...props}/>
        case 'multiselect':
            return <MultiSelectInput className={'field col-12  md:col-4'} key={field} {...props}/>
        default:
            return <TextInput className={'field col-12 md:col-4'} key={field} {...props}/>
    }
}
