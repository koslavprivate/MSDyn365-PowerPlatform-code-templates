import { Image } from '@fluentui/react';
import * as React from 'react';
import { CellRendererOverrides } from '../types';

export const cellRendererOverrides: CellRendererOverrides = {
    ["Text"]: (props, col) => {
        // Only override the cell renderer for the CreditLimit column
        const imageRegex = /\.(png|jpg|jpeg|svg|gif|webp|bmp)$/i;
        if (props.formattedValue && imageRegex.test(props.formattedValue)) {
            try {
                const u = new URL(props.formattedValue);
                return <Image src={u.href} alt={"image"} style={{ width: '40%', height: '40%' }} />;
            } catch (e) {
                return null;
            }
        }
        return null;
    }
};
