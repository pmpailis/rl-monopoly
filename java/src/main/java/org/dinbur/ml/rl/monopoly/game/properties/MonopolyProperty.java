package org.dinbur.ml.rl.monopoly.game.properties;


import com.google.gson.Gson;
import com.google.gson.GsonBuilder;
import org.dinbur.ml.rl.monopoly.utils.RuntimeTypeAdapterFactory;

import java.util.List;

public abstract class MonopolyProperty {

    private String name;
    private int position;
    private int price;
    private List<Integer> rent;
    private int mortgage;
    private int houseCost;
    private int hotelCost;
    private String group;

    public static Gson createGson() {

        RuntimeTypeAdapterFactory<MonopolyProperty> typeAdapterFactory = RuntimeTypeAdapterFactory
                .of(MonopolyProperty.class, "type")
                .registerSubtype(RailwayProperty.class, "railways")
                .registerSubtype(UtilityProperty.class, "utilities")
                .registerSubtype(StandardProperty.class, "standard");

        return new GsonBuilder().registerTypeAdapterFactory(typeAdapterFactory)
                .create();

    }
}
